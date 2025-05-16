public interface IFileParser
{
    /// <summary>
    /// Parses the input stream and returns a collection of records.
    /// </summary>
    /// <param name="fileStream">The file stream to parse.</param>
    /// <returns>List of parsed file records.</returns>
    Task<IEnumerable<FileRecord>> ParseAsync(Stream fileStream);
}