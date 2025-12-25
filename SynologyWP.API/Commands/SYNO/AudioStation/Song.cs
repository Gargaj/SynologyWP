using System.Collections.Generic;

namespace SynologyWP.API.Commands.SYNO.AudioStation
{
  public class SongList : ICommand
  {
    public string APIName => "SYNO.AudioStation.Song";
    public int APIVersion => 3;
    public string APIMethod => "list";

    [QueryParameter]
    public string album { get; set; }
    [QueryParameter]
    public string album_artist { get; set; }
    [QueryParameter]
    public string sort_by { get; set; }
    [QueryParameter]
    public string sort_direction { get; set; }
    [QueryParameter]
    public string additional { get; set; }
  }

  public class SongAudio
  {
    public int bitrate;
    public int channel;
    public string codec;
    public string container;
    public int duration;
    public int filesize;
    public int frequency;
  }

  public class SongRating
  {
    public int rating;
  }

  public class SongTag
  {
    public string album;
    public string album_artist;
    public string artist;
    public string comment;
    public string composer;
    public int disc;
    public string genre;
    public int track;
    public int year;
  }

  public class Additional
  {
    public SongAudio song_audio;
    public SongRating song_rating;
    public SongTag song_tag;
  }

  public class Song
  {
    public string id;
    public string path;
    public string title;
    public string type;
    public Additional additional;
  }

  public class SongListResult : IResult
  {
    public List<Song> songs;
    public int offset;
    public int total;
  }
}
