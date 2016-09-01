namespace FastMapper
{
    public class PropMap
    {
        public bool IsIgnore { get; private set; }

        public void Ignore()
        {
            IsIgnore = true;
        }

        public string MapProp { get; set; }

    }
}