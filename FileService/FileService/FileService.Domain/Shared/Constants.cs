namespace FileService.Domain.Shared;

public static class Constants
{
    public const int MAX_LENGTH_FILE_NAME = 255;
    public const int MAX_LENGTH_MIME_TYPE = 255;
    public const int MAX_LENGTH_STORAGE_KEY = 512;
    public const int MAX_LENGTH_ENTITY_TYPE = 64;
    public const int SHA256_LENGTH = 64;
    public const int MAX_LENGTH_ETAG = 512;
    public const int MAX_LENGTH_FAILURE_CODE = 100;
    public const int MAX_LENGTH_FAILURE_MESSAGE = 1000;

    public static class FileSize
    {
        public const long MAX_AVATAR_SIZE = 5L * 1024 * 1024;
        public const long MAX_COURSE_COVER_SIZE = 10L * 1024 * 1024;
        public const long MAX_MARKDOWN_IMAGE_SIZE = 10L * 1024 * 1024;
        public const long MAX_ATTACHMENT_SIZE = 25L * 1024 * 1024;
        public const long MAX_LESSON_VIDEO_SIZE = 2L * 1024 * 1024 * 1024;
        public const long MAX_PREVIEW_SIZE = 5L * 1024 * 1024;
    }
}