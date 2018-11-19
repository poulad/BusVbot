using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Wit.Ai.Client;
using Wit.Ai.Client.Types;

namespace BusV.Ops
{
    public class NlpService : INlpService
    {
        private readonly IWitClient _witClient;

        public NlpService(
            IWitClient witClient
        )
        {
            _witClient = witClient;
        }

        public async Task<Meaning> ProcessVoiceAsync(
            string filePath,
            string mimeType,
            CancellationToken cancellationToken
        )
        {
            // ToDo if (mimeType == "audio/ogg")

            // ToDo remove this wav file at the end
            string waveFileName = Path.GetTempFileName();

            string args = $"-f ogg -i \"{filePath}\" -nostdin -nostats -loglevel error -y -f wav \"{waveFileName}\"";
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("ffmpeg", args)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = false,
                    UseShellExecute = false,
                },
            };

            // ToDo try-finally -> dispose process
            process.Start();
            await Task.Run(() => process.WaitForExit(3_500), cancellationToken)
                .ConfigureAwait(false);
            if (process.HasExited)
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (process.ExitCode == 0)
                {
                    Meaning meaning;
                    using (var voiceFile = File.OpenRead(waveFileName))
                    {
                        meaning = await _witClient.SendAudioAsync(
                            voiceFile,
                            "audio/wave",
                            cancellationToken: cancellationToken
                        ).ConfigureAwait(false);
                    }

                    return meaning;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
