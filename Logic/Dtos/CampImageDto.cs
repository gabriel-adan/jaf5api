namespace Logic.Dtos
{
    public class CampImageDto : EntityDto
    {
        public string FileUrl { get; }

        public CampImageDto(int id, string fileUrl) : base (id)
        {
            FileUrl = fileUrl;
        }
    }
}
