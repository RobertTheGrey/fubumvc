using System.IO;
using System.Text;
using FubuMVC.Core.View;
using FubuMVC.UI;
using Spark.Web.FubuMVC.ViewCreation;

namespace Spark.Web.FubuMVC.Extensions
{
    public static class SparkViewExtensions
    {
        public static void WritePartial<TInputModel>(this IFubuPage page, TInputModel model) where TInputModel : class
        {
            writePartial(page, model);
        }

        public static void WritePartial<TInputModel>(this IFubuPage page) where TInputModel : class
        {
            writePartial(page, default(TInputModel));
        }

        public static string GetPartial<TInputModel>(this IFubuPage page, TInputModel model) where TInputModel : class
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            writePartial(page, model, sw);
            return sb.ToString();
        }

        public static string GetPartial<TInputModel>(this IFubuPage page) where TInputModel : class
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            writePartial(page, default(TInputModel), sw);
            return sb.ToString();
        }

        private static void writePartial<TInputModel>(IFubuPage page, TInputModel model) where TInputModel : class
        {
            var view = (SparkViewBase)page;
            writePartial(page, model, view.Output);
        }
        private static void writePartial<TInputModel>(IFubuPage page, TInputModel model, TextWriter writer) where TInputModel : class
        {
            var writerContext = page.Get<ISparkWriterContext>();
            try
            {
                writerContext.Set(writer);
                page.Partial(model);
            }
            finally
            {
                writerContext.Reset();
            } 
        }
    }
}