using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Compression;
using System.IO;
using NLayer;
using NVorbis;


namespace OsuSongProcessorNS
{
    public class OsuSongProcessor
    {
        /// <summary> 
        /// Parse supported OSZ file from path
        /// </summary> 
        ///<param name="OsuFile">String of OSZ file path (loading OSU files directly is NOT supported), this should be in Unity's Application.persistentDataPath + /Songs/ folder</param>
        public Song ExtractSong(string OsuFormatFilePath)
        {
            Song output = new Song();

            Debug.Log("(-) Processor Data Path: " + Application.persistentDataPath);
            Debug.Log("(-) Processing file at Path: " + OsuFormatFilePath);
            Debug.Log("(-) Scanning OSZ for files");
            System.IO.Compression.ZipFile.OpenRead(OsuFormatFilePath);
            using (ZipArchive OsuArchive = ZipFile.OpenRead(OsuFormatFilePath))
            {
                foreach (ZipArchiveEntry entry in OsuArchive.Entries)
                {
                    if (entry.FullName.EndsWith(".osu"))
                    {
                        Debug.Log("(-) Found OSU file: " + entry.FullName);
                        using (StreamReader sr = new StreamReader(entry.Open()))
                        {
                            Debug.Log("(-) Processing OSU file");
                            string OsuFile = sr.ReadToEnd();
                            output.beats = ParseOsuFile(OsuFile);
                        }
                    }
                    else if (entry.FullName.EndsWith(".mp3"))
                    {
                        Debug.Log("(-) Found MP3 file: " + entry.FullName);
                        using (StreamReader sr = new StreamReader(entry.Open()))
                        {
                            Debug.Log("(-) Processing MP3 file");
                            string AudioFilePath = Application.temporaryCachePath + "/CurrentSong.mp3";
                            Debug.Log("(-) Creating Temporary song file at: " + AudioFilePath);
                            entry.ExtractToFile(AudioFilePath, true);

                            output.Music = MpegToAudioClip(AudioFilePath);
                        }
                    }
                    else
                    {
                        //Debug.Log("(DATA) Found: " + entry.FullName);
                    }
                }
            }
            //Final Song Setup
            output.Source = SongSource.OSU;

            return output;
        }
        public Song ExtractSong(string OsuFilePath, string AudioFilePath)
        {
            Song output = new Song();

            Debug.Log("(-) Processor Data Path: " + Application.persistentDataPath);

            Debug.Log("(-) Processing OSU file at Path: " + OsuFilePath);
            using (StreamReader sr = new StreamReader(OsuFilePath))
            {
                Debug.Log("(-) Processing OSU file");
                string OsuFile = sr.ReadToEnd();
                output.beats = ParseOsuFile(OsuFile);
            }

            Debug.Log("(-) Processing audio file at Path: " + AudioFilePath);
            output.Music = OggToAudioClip(AudioFilePath);

            output.Source = SongSource.OSU;

            return output;
        }

        /// <summary> 
        /// Parse OSU file as string and turn into list of beats
        /// </summary> 
        ///<param name="OsuFile">String of OSU file contents</param>
        List<Beat> ParseOsuFile(string OsuFile)
        {
            List<Beat> Beats = new List<Beat>();
            int HitObjectsHeaderPos = -1;
            string[] lineSeparatedOsuFile = OsuFile.Split('\n');
            //Loop through list of lines and find headers
            for (int i = 0; i < lineSeparatedOsuFile.Length; i++)
            {
                string line = lineSeparatedOsuFile[i];
                if (line.Contains("[HitObjects]"))
                {
                    HitObjectsHeaderPos = i;
                }
            }
            //Store HitObjects starting at HitObjectsHeader+1 to end of file
            for (int i = HitObjectsHeaderPos + 1; i < lineSeparatedOsuFile.Length; i++)
            {
                string[] parsedHitObject = lineSeparatedOsuFile[i].Split(',');
                /// https://osu.ppy.sh/wiki/en/Client/File_formats/osu_%28file_format%29
                /// Hit object syntax: x,y,time,type,hitSound,objectParams,hitSample
                // x (Integer) and y (Integer): Position in osu! pixels of the object.
                // time (Integer): Time when the object is to be hit, in milliseconds from the beginning of the beatmap's audio.
                // type (Integer): Bit flags indicating the type of the object. See the type section.
                // hitSound (Integer): Bit flags indicating the hitsound applied to the object. See the hitsounds section.
                // objectParams (Comma-separated list): Extra parameters specific to the object's type.
                // hitSample (Colon-separated list): Information about which samples are played when the object is hit. It is closely related to hitSound; see the hitsounds section. If it is not written, it defaults to 0:0:0:0:.
                if (parsedHitObject.Length > 5)
                {
                    Beat currentBeat = new Beat();
                    currentBeat.time = int.Parse(parsedHitObject[2]);
                    Beats.Add(currentBeat);
                }
            }
            Debug.Log("(-) Found " + Beats.Count + " Beats in OSU File");
            return Beats;
        }
        /// <summary> 
        /// Use NLayer to sample MP3 file and store as AudioClip.
        /// Wayyy better than using UnityWebRequests/WWW
        /// </summary> 
        ///<param name="mpegFilePath">String of mpeg file path, this should be in Unity's Application.temporaryCachePath or Application.persistentDataPath</param>
        AudioClip MpegToAudioClip(string mpegFilePath)
        {
            MpegFile mpegFile = new MpegFile(mpegFilePath);
            AudioClip clip = AudioClip.Create("CurrentSong",
                (int)(mpegFile.Length / sizeof(float) / mpegFile.Channels),
                mpegFile.Channels,
                mpegFile.SampleRate,
                true,
                data => { int actualReadCount = mpegFile.ReadSamples(data, 0, data.Length); },
                position => { mpegFile = new MpegFile(mpegFilePath); });
            return clip;
        }
        /// <summary> 
        /// Use NVorbis to sample MP3 file and store as AudioClip.
        /// </summary> 
        ///<param name="vorbisFilePath">String of vorbis (ogg) file path, this should be in Unity's Application.temporaryCachePath or Application.persistentDataPath</param>
        AudioClip OggToAudioClip(string vorbisFilePath)
        {
            VorbisReader vorbis = new VorbisReader(vorbisFilePath);
            AudioClip clip = AudioClip.Create("CurrentSong",
                (int)(vorbis.TotalSamples),
                vorbis.Channels,
                vorbis.SampleRate,
                true,
                data => { int actualReadCount = vorbis.ReadSamples(data, 0, data.Length); },
                position => {vorbis = new VorbisReader(vorbisFilePath); });
            return clip;
        }
        
    }

}