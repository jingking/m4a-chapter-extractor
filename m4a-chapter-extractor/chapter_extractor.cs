using System;
using System.IO;


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
                        int[] timetosample = null;
                        //int[] samplesize; //= null;
                        int[] chunkoffset = null;

                        long start = 0;
                        fs.Seek(start, SeekOrigin.Begin);
                        int len = StreamUtils.GetAtomSize(fs);
                        fs.Seek(4, SeekOrigin.Current);
                        var ftyp = StreamUtils.GetAtomType(fs);

                        switch (ftyp)
                        {
                            case "m4a ":
                            case "M4A ":
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
                            int trakcount = 0;
                            while (!done && fs.Position < fs.Length)
                            {
                                len = StreamUtils.GetAtomSize(fs);
                                string type = StreamUtils.GetAtomType(fs);

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

                                    //move into the following atom boxes
                                    case "moov":
                                    case "mdia":
                                    case "minf":
                                    case "stbl":

                                        fs.Seek(0, SeekOrigin.Current);
                                        break;


                                    case "trak":
                                        trakcount++;
                                        if (trakcount < 2)
                                            fs.Seek(len - 8, SeekOrigin.Current);
                                        else
                                            fs.Seek(0, SeekOrigin.Current);
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
                                    chapters[i] = new Chapter(StreamUtils.GetChapter(fs), (i < 1) ? 0 : chapters[i - 1].EndTime,  t/ 1000.0);
                                    
                                };
                                /*
                                 * output format
                                 *     chapters: [
                                                  { title, startTime, endTime }, // the first chapter
                                                  { title, startTime, endTime }, // the second chatper
                                                ],
                                */
                                Console.WriteLine("[");
                                for (var i = 0; i < chapters.Length; i++)
                                {

                                    Console.WriteLine("{\"" + chapters[i].Title + "\"," + chapters[i].StartTime + "," + chapters[i].EndTime + "},");

                                };
                                Console.WriteLine("]");
                                Console.ReadLine();
                            }

                        }

                    }



                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


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
