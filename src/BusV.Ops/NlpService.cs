using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wit.Ai.Client;
using Wit.Ai.Client.Types;

namespace BusV.Ops
{
    public class NlpService : INlpService
    {
        private readonly IWitClient _witClient;
        private readonly ILogger _logger;

        public NlpService(
            IWitClient witClient,
            ILogger<NlpService> logger
        )
        {
            _witClient = witClient;
            _logger = logger;
        }

        public Task<Meaning> ProcessTextAsync(string text, CancellationToken cancellationToken)
            => _witClient.GetSentenceMeaningAsync(text, cancellationToken: cancellationToken);

        public async Task<Meaning> ProcessVoiceAsync(
            Stream audioStream,
            string mimeType,
            CancellationToken cancellationToken
        )
        {
            Meaning meaning;

            if (mimeType.Contains("ogg"))
            {
                var ffMpegProcess = ConvertUsingFFMpeg(audioStream, "ogg", "wav");

                using (ffMpegProcess)
                {
                    meaning = await _witClient.GetAudioMeaningAsync(
                        ffMpegProcess.StandardOutput.BaseStream,
                        "audio/wave",
                        cancellationToken: cancellationToken
                    ).ConfigureAwait(false);
                }
            }
            else
            {
                meaning = await _witClient.GetAudioMeaningAsync(
                    audioStream,
                    mimeType,
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
            }

            return meaning;
        }

        private Process ConvertUsingFFMpeg(
            Stream inputStream,
            string inputFormat,
            string outputFormat
        )
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo(
                    "ffmpeg",
                    $"-f {inputFormat} -i pipe:0 -loglevel error -f {outputFormat} -"
                )
                {
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                }
            };
            process.Start();

            inputStream.CopyTo(process.StandardInput.BaseStream);
            process.StandardInput.Flush();
            process.StandardInput.Close();

            return process;
        }
    }
}
