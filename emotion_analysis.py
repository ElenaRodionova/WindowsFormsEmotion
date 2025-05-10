import warnings
import sys
import librosa
import soundfile as sf
import numpy as np
import os
import json
import webrtcvad
import gigaam

def split_audio_by_voice_activity(audio_path, output_dir, model, vad_aggressiveness=2, frame_duration=30, min_segment_length=0.5, min_gap_length=1):
    """
    Разбивает аудиофайл на сегменты на основе детекции голосовой активности (VAD) и выполняет распознавание эмоций для каждого сегмента.
    Сначала объединяет сегменты, короче чем min_segment_length, затем объединяет сегменты с промежутками, короче чем min_gap_length.

    Args:
        audio_path (str): Путь к входному аудиофайлу.
        output_dir (str): Директория для сохранения сегментированных аудиофайлов.
        model: Объект с методом `get_probs`, который принимает путь к аудиофайлу и возвращает словарь вероятностей эмоций.
        vad_aggressiveness (int, optional): Уровень агрессивности VAD (0-3). Более высокие значения более агрессивны. По умолчанию 3.
        frame_duration (int, optional): Длительность кадра в миллисекундах для VAD. По умолчанию 30.
        min_segment_length (float, optional): Минимальная длина сегмента в секундах. Сегменты короче этого значения будут объединены с соседними. По умолчанию 0,5.
        min_gap_length (float, optional): Минимальная длина промежутка в секундах между сегментами для предотвращения объединения. По умолчанию 1.
        reduce_noise (bool, optional):  Применять ли шумоподавление к аудио. По умолчанию True.
        
    Returns:
        list: Список словарей, где каждый словарь содержит время начала, время окончания и предсказанную эмоцию для сегмента. Возвращает пустой список в случае ошибки.
    """
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)
    results = []
    try:
        audio, sample_rate = librosa.load(audio_path, sr=None)  # Сохраняем исходную частоту дискретизации
        if len(audio.shape) > 1:
            audio = librosa.to_mono(audio)
        if sample_rate != 16000:
            audio = librosa.resample(audio, orig_sr=sample_rate, target_sr=16000)
            sample_rate = 16000
        vad = webrtcvad.Vad(vad_aggressiveness)
        audio_int16 = np.int16(audio * 32767)  # Преобразуем float в int16 (диапазон -32768..32767)
        audio_bytes = audio_int16.tobytes()
        frame_bytes = int(frame_duration * sample_rate / 1000 * 2)

        frames_bytes = []
        for i in range(0, len(audio_bytes), frame_bytes):
            frame = audio_bytes[i:i + frame_bytes]
            if len(frame) < frame_bytes:
                continue
            frames_bytes.append(frame)
        is_speech = []
        for i, frame in enumerate(frames_bytes):
            try:
                is_speech.append(vad.is_speech(frame, sample_rate))
            except Exception as e:
                print(f"Ошибка при обработке кадра {i}: {e}")
                is_speech.append(False)  # Обрабатываем ошибку, считая кадр тишиной
        segments = []
        start_time = 0
        in_segment = False
        for i, speech in enumerate(is_speech):
            current_time = i * frame_duration / 1000.0
            if speech and not in_segment:
                start_time = current_time
                in_segment = True
            elif not speech and in_segment:
                end_time = current_time
                segment = audio[int(start_time * sample_rate):int(end_time * sample_rate)]
                segments.append((start_time, end_time, segment))
                in_segment = False
        if in_segment:
            end_time = len(audio) / sample_rate
            segment = audio[int(start_time * sample_rate):]
            segments.append((start_time, end_time, segment))

        # Объединяем короткие сегменты
        merged_segments = []
        i = 0
        while i < len(segments):
            start_time, end_time, segment = segments[i]
            segment_duration = end_time - start_time

            if segment_duration < min_segment_length:
                # Пытаемся объединить со следующим сегментом
                if i + 1 < len(segments):
                    next_start_time, next_end_time, next_segment = segments[i + 1]
                    combined_segment = np.concatenate((segment, next_segment))
                    combined_end_time = next_end_time
                    merged_segments.append((start_time, combined_end_time, combined_segment))
                    i += 2  # Пропускаем оба сегмента
                elif len(merged_segments) > 0: # Пытаемся объединить с предыдущим
                    prev_start_time, prev_end_time, prev_segment = merged_segments[-1]
                    merged_segments.pop()
                    combined_segment = np.concatenate((prev_segment, segment))
                    combined_start_time = prev_start_time
                    merged_segments.append((combined_start_time, end_time, combined_segment))
                    i +=1
                else:
                     merged_segments.append((start_time, end_time, segment)) 
                     i+=1
            else:
                merged_segments.append((start_time, end_time, segment))
                i += 1

        # Объединяем сегменты с короткими промежутками
        final_segments = []
        i = 0
        while i < len(merged_segments):
            start_time, end_time, segment = merged_segments[i]

            # Проверяем на короткий промежуток и объединяем со следующим сегментом
            if i + 1 < len(merged_segments):
                next_start_time, next_end_time, next_segment = merged_segments[i + 1]
                gap_length = next_start_time - end_time
                if gap_length <= min_gap_length:
                    combined_segment = np.concatenate((segment, next_segment))
                    combined_end_time = next_end_time
                    final_segments.append((start_time, combined_end_time, combined_segment))
                    i += 2  # Пропускаем оба сегмента
                else:
                    final_segments.append((start_time, end_time, segment))  # Добавляем сегмент в результаты
                    i += 1
            else:
                final_segments.append((start_time, end_time, segment))
                i += 1


        # Анализируем каждый сегмент
        for i, (start_time, end_time, segment) in enumerate(final_segments):
            segment_file_name = f"segment_{i}.wav"
            segment_file_path = os.path.join(output_dir, segment_file_name)
            sf.write(segment_file_path, segment, sample_rate)
            try:
                emotion2prob = model.get_probs(segment_file_path)
                best_emotion = max(emotion2prob, key=emotion2prob.get)
                results.append({
                    'start_time': start_time,
                    'end_time': end_time,
                    'emotion': best_emotion
                })
            except Exception as e:
                print(f"Ошибка при анализе эмоций для сегмента {i}: {e}")
                continue
            os.remove(segment_file_path)

        return results
    except Exception as e:
        print(f"Ошибка при обработке аудио: {e}")
        return []

    
if __name__ == '__main__':
    
  if len(sys.argv) > 2:
    audio_path = sys.argv[1] 
    output_dir = sys.argv[2]
    
    try:
      with warnings.catch_warnings():
        warnings.simplefilter("ignore")
        
        model = gigaam.load_model("emo")
        segment_data = split_audio_by_voice_activity(audio_path, output_dir, model)
        json_output = json.dumps(segment_data, indent=4)
        print(json_output)
        
    except Exception as e:
      print(f"Ошибка во время анализа: {e}")