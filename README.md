# SpeechApp

A Windows desktop application for recording speech and converting it to text, and for reading typed text aloud. Transcription uses a local multilingual Whisper model and playback uses installed Windows voices.

## Features

- Record up to two minutes through the default microphone, then stop to transcribe in Turkish or English.
- Cancel transcription, clear the transcript, or select and copy the result.
- Read typed text aloud with an installed voice. Pause, resume, stop, or clear the input.
- Follow the current activity and any errors below each panel.

## Requirements

- Windows x64 and the .NET 10 SDK.
- [Microsoft Visual C++ Redistributable x64](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist) for the native Whisper runtime.
- A default recording device for microphone input and an audio output device for playback.
- A supported Windows voice for each language you want to read aloud.
- A local GGML Whisper model for transcription. Use the multilingual model, such as `ggml-base.bin`.

The app uses Windows Forms, [System.Speech](https://www.nuget.org/packages/System.Speech/10.0.15), and [Whisper.net with its CPU runtime](https://github.com/sandrohanea/whisper.net).

## Run from source

In PowerShell at the repository root, run:

```powershell
dotnet run --project src/SpeechApp/SpeechApp.csproj
```

Text-to-speech is available as soon as the app finds an installed voice. To enable transcription, download the [multilingual Whisper base model](https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin) (about 141 MiB). Use **Choose model** to open the downloaded file, or save it as `models/ggml-base.bin` and launch the app from the repository root. The model directory is ignored by Git.

## Using the app

1. In **Speech to Text**, choose Turkish or English, click **Start recording**, and speak into the default microphone. Grant microphone access in Windows if needed. Click **Stop** to start transcription, or let it start automatically at the time limit.
2. Review the result in the transcript. Choose **Cancel** while transcription is running or **Clear** to remove previous results when idle.
3. In **Text to Speech**, type some text, choose an installed voice, and click **Read aloud**. Use **Pause**, **Resume**, or **Stop** to control playback.

The app runs one audio operation at a time. Recording is held in memory, and the model runs on your computer. Accuracy varies with the model, language, and recording quality.
