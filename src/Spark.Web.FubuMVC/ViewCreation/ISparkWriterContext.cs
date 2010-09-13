using System;
using System.IO;
using System.Web;

namespace Spark.Web.FubuMVC.ViewCreation
{
    public interface ISparkWriterContext
    {
        TextWriter Writer { get; }
        void Set(TextWriter writer);
        void Reset();
    }

    public class SparkWriterContext : ISparkWriterContext
    {
        private readonly HttpContextBase _httpContext;
        private TextWriter _writer;
        public SparkWriterContext(HttpContextBase httpContext)
        {
            _httpContext = httpContext;
            _writer = httpContext.Response.Output;
        }

        public TextWriter Writer
        {
            get { return _writer; }
        }

        public void Set(TextWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");
            _writer = writer;
        }

        public void Reset()
        {
            _writer = _httpContext.Response.Output;
        }
    }
}