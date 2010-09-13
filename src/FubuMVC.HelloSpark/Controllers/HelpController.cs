namespace FubuMVC.HelloSpark.Controllers
{
    public class HelpController
    {
        public HelpViewModel Display(HelpViewModel model)
        {
            return model;
        }
    }

    public class HelpViewModel
    {
        private string _rawUrl;
        public string RawUrl
        {
            get { return _rawUrl; }
            set { _rawUrl = value; }
        }
    }

}