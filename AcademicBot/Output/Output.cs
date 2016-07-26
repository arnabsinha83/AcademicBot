using System.Text;
using AcademicBot.Controllers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace AcademicBot.Output
{
    public class JsonFormatter
    {
        private EvaluateModel.Rootobject obj { get; set; }

        private int MaxAuthors { get; set; }

        public string FormatEvaluateModel(string evalModel, int maxAuthors = 3)
        {
            obj = JsonConvert.DeserializeObject<EvaluateModel.Rootobject>(evalModel);
            this.MaxAuthors = maxAuthors;

            return FormatStringFromObj();
        }

        public string FormatEvaluateModel(EvaluateModel.Rootobject evalModel, int maxAuthors = 3)
        {
            obj = evalModel;
            this.MaxAuthors = maxAuthors;

            return FormatStringFromObj();
        }

        private string FormatStringFromObj()
        {
            return HandleEntities(obj.entities);
        }

        private string HandleEntities(EvaluateModel.Entity[] entities)
        {
            List<string> entityMarkdownList = new List<string>();
            foreach(var e in entities)
            {
                entityMarkdownList.Add(HandleEntity(e));
            }
            return string.Join("\n\n\n\n", entityMarkdownList);
        }

        private string HandleEntity(EvaluateModel.Entity entity)
        {
            string reply = string.Format("\"{0}\",{1},{2}",
                            HandleTitle(entity.Ti, entity.Id),
                            HandleAuthors(entity.AA),
                            HandleYear(entity.Y));
            return reply;
        }

        private string HandleYear(int Y)
        {
            return string.Format("{0}", Y);
        }

        private string HandleTitle(string Ti, Int64 Id)
        {
            return string.Format("**[{0}]{1}**", Ti, CreateEntityLink(Id));
        }

        private string HandleAuthors(EvaluateModel.AA[] authors)
        {
            List<string> authorList = new List<string>();
            int authorDisplayLength = authors.Length > MaxAuthors ? MaxAuthors : authors.Length;
            for (var index = 0; index < authorDisplayLength; index++)
            {
                authorList.Add(HandleAuthor(authors[index]));
            }
            string reply = string.Join(", ", authorList);
            if(authors.Length > MaxAuthors)
            {
                reply += "...";
            }
            return reply;
        }

        private string HandleAuthor(EvaluateModel.AA author)
        {
            return string.Format("*[{0}]{1}*", author.AuN, CreateEntityLink(author.AuId));
        }

        private string CreateEntityLink(long Id)
        {
            if(Id <= 0)
            {
                return string.Empty;
            }
            return string.Format("(https://academic.microsoft.com/#/detail/{0})", Id);
        }
    }
}
