using System;

namespace M4a_chapter_extractor
{
    public class Chapter
    {
        public string Title { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }

        public Chapter(string str, double start, double end)
        {
            this.Title = str;
            this.StartTime = start;
            this.EndTime = end;
        }
    }
}