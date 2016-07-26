using System.Text;
using AcademicBot.Controllers;
using Newtonsoft.Json;

namespace AcademicBot.Output
{
    public class JsonFormatter
    {
        readonly StringBuilder _builder = new StringBuilder();

        private EvaluateModel.Rootobject Rootobject { get; set; }

        private int MaxAuthors { get; set; }

        public string FormatEvaluateModel(string evalModel, int maxAuthors = 3)
        {
            Rootobject = JsonConvert.DeserializeObject<EvaluateModel.Rootobject>(evalModel);
            this.MaxAuthors = maxAuthors;

            return FormatStringFromObj();
        }

        public string FormatEvaluateModel(EvaluateModel.Rootobject evalModel, int maxAuthors = 3)
        {
            Rootobject = evalModel;
            this.MaxAuthors = maxAuthors;

            return FormatStringFromObj();
        }

        private string FormatStringFromObj()
        {
            HandleExpr();
            HandleEntities();
            return _builder.ToString();
        }

        private void HandleExpr()
        {
            if (Rootobject.expr.Contains("Composite(AA.AfN=="))
            {
                _builder.AppendLine("**Organization:**" + Rootobject.expr.Replace("Composite(AA.AfN==", "").Replace(")", ""));
            }

            if (Rootobject.expr.Contains("Composite(AA.AuN=="))
            {
                _builder.AppendLine("**Author:**" + Rootobject.expr.Replace("Composite(AA.AuN==", "").Replace(")", ""));
            }
        }

        private void HandleEntities()
        {
            foreach (var entity in Rootobject.entities)
            {
                _builder.AppendLine($"**Title**:{entity.Ti}, **Year**:{entity.Y} [link](https://academic.microsoft.com/#/detail/" + entity.Id + ")");
                HandleAuthor(entity);
            }
        }

        private void HandleAuthor(EvaluateModel.Entity entity)
        {
            _builder.Append("**Author(s):**");
            for (var index = 0; index < (entity.AA.Length > MaxAuthors ? MaxAuthors : entity.AA.Length); index++)
            {
                var author = entity.AA[index];
                _builder.Append($"{author.AuN},");
            }

            _builder.AppendLine();
        }

    }
}
