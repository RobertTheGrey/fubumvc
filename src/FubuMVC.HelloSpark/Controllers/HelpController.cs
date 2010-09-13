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
        public string RawUrl { get; set; }
    }

}