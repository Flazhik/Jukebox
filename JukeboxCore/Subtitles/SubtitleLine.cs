namespace JukeboxCore.Subtitles;

public readonly struct SubtitleLine(float start, float end, string text)
{
    public float Start { get; } = start;
    public float End { get; } = end;
    public string Text { get; } = text;
}
