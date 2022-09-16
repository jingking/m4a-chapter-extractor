using System;
using System.IO;
using System.Text.Json;

namespace M4a_chapter_extractor
{

    class chapter_extractor
    {
        static void Main(string[] args)
        {
            string filepath = "";

            if (args.Length > 0)
                filepath = args[0];
            else
            { 
                Console.Write("Please provide a valid file path.");
                return;
            }

            try
            {// Read file
                using (FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
                {

                    long MaxLength = (int)fs.Length;

                    if (fs.Length > 0)
                    {
                        int[]? timetosample = null;
                        //int[]? samplesize = null;
                        int[]? chunkoffset = null;

                        long start = 0;
                        fs.Seek(start, SeekOrigin.Begin);
                        int len = StreamUtils.GetAtomSize(fs);
                        fs.Seek(4, SeekOrigin.Current);
                        var ftyp = StreamUtils.GetAtomType(fs);

                        switch (ftyp)
                        {
                            case "M4A ":
                            case "m4a ":
                            case "mp42":
                                ftyp = "m4a";
                                break;
                            default:
                                Console.WriteLine("Not designed for this file type.");
                                return;
                        }


                        if (!string.IsNullOrEmpty(ftyp))
                        {
                            fs.Seek(len - 12, SeekOrigin.Current);//move pointer to the next atom box

                            bool done = false;

                            while (!done && fs.Position < fs.Length)
                            {
                                len = StreamUtils.GetAtomSize(fs);
                                string type = StreamUtils.GetAtomType(fs);
                                //int trakid = 0;
                                switch (type)
                                {
                                    case "stts"://time   
                                        timetosample = StreamUtils.GetTimeToSample(fs);
                                        break;

                                    //case "stsz"://size
                                    //    samplesize = StreamUtils.GetSampleSize(fs);
                                    //    break;

                                    case "stco"://offset
                                        chunkoffset = StreamUtils.GetChunkOffset(fs);
                                        break;

                                    //"chap" atom (leaf of a "tref") might hold the stream index to the chapter track? 
                                    //case "chap":
                                    //    trakid = StreamUtils.GetAtomId(fs);
                                    //    break;

                                    //move into the following atom boxes
                                    //case "tref":
                                    case "moov":
                                    case "minf":
                                    case "stbl":
                                    case "trak":
                                        fs.Seek(0, SeekOrigin.Current);
                                        break;

                                    case "mdia":
                                        fs.Seek(48, SeekOrigin.Current);
                                        if (StreamUtils.GetAtomType(fs).ToLower() == "text")//find the "text" track
                                            fs.Seek(-20, SeekOrigin.Current);
                                        else
                                            fs.Seek(len - 52, SeekOrigin.Current);
                                        break;
                                    //skip the following atom boxes
                                    case "free":
                                    case "mdat":
                                    case "uuid":
                                    case "mvhd":
                                    case "stsc":

                                    default:
                                        fs.Seek(len - 8, SeekOrigin.Current);
                                        break;
                                }
                            }

                            //get chapter strings
                            if ((timetosample != null && chunkoffset != null) && (timetosample.Length == chunkoffset.Length))
                            {
                                Chapter[] chapters = new Chapter[timetosample.Length];
                                int t = 0;
                                for (var i = 0; i < chapters.Length; i++)
                                {
                                    fs.Position = chunkoffset[i];
                                    t = timetosample[i] + t;
                                    chapters[i] = new Chapter(StreamUtils.GetChapter(fs), (i < 1) ? 0 : chapters[i - 1].EndTime, t / 1000.0);
                                };
                                Console.WriteLine(JsonSerializer.Serialize(chapters, new JsonSerializerOptions { WriteIndented = true }));
                                /*
                                 * format
                                 *     chapters: [
                                                  { title, startTime, endTime }, // the first chapter
                                                  { title, startTime, endTime }, // the second chatper
                                                ],
                                */
                            }
                            else
                                Console.WriteLine("Can not find valid chapter information");

                        }

                    }



                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            //Console.ReadLine();

            // Debug - Dump 16 bytes per line
            //for (int i = 0; i < len; i += 16)
            //{
            //    var cnt = Math.Min(16, len - i);
            //    var line = new byte[cnt];
            //    Array.Copy(bits, i, line, 0, cnt);
            //    // Write address + hex + ascii
            //    Console.Write("{0:X6}  ", i);
            //    Console.Write(BitConverter.ToString(line));
            //    Console.Write("  ");
            //    // Convert non-ascii characters to .
            //    for (int j = 0; j < cnt; ++j)
            //        if (line[j] < 0x20 || line[j] > 0x7f) line[j] = (byte)'.';
            //    Console.WriteLine(Encoding.ASCII.GetString(line));
            //}
        }
    }
}
