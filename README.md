# m4a-chapter-extractor
get chapter information from podcast files (in *.m4a format)


##Usage 
````
m4a-chapter-extractor.exe "yourpodcastfilenamehere.m4a" 
````
<br>
will output all chapters in json string format
```` 
                                 *     chapters: [
                                                  { title, startTime, endTime }, // the first chapter
                                                  { title, startTime, endTime }, // the second chatper
                                                  ...
                                                ],
````
<img src="readme_usage_screenshot.png" width="600" />

If you want to save the json object strings into a txt file,
````
m4a-chapter-extractor.exe "yourpodcastfilenamehere.m4a" >chapter.txt
````
