using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketTest
{
    public class VoiceBarrage
    {
        PromptBuilder _current;
        Queue<PromptBuilder> _promptQueue;
        SpeechSynthesizer _speechSynthesizer;
        public VoiceBarrage()
        {
            _promptQueue = new Queue<PromptBuilder>();
            AutoPlay();
        }
        public void AddVoiceText(string message)
        {
            PromptBuilder promptBuilder = new PromptBuilder();
            promptBuilder.AppendText(message);
            _promptQueue.Enqueue(promptBuilder);
        }

        private void AutoPlay()
        {
            ThreadPool.QueueUserWorkItem(Play);
        }

        private void Play(object state)
        {
            while (true)
            {
                if (_promptQueue.Count > 0)
                {
                    lock (_promptQueue)
                    {
                        _current = _promptQueue.Dequeue();
                    }
                    try
                    {
                        _speechSynthesizer = new SpeechSynthesizer();
                        _speechSynthesizer.SetOutputToDefaultAudioDevice();
                        _speechSynthesizer.Rate = 10;
                        _speechSynthesizer.Speak(_current);
                        _speechSynthesizer.Dispose();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                Thread.Sleep(100);
            }
        }
    }
}
