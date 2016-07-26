using System.Text;
using AcademicBot.Controllers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace AcademicBot.Output
{
    public class JsonFormatter
    {
        private EvaluateModel.Rootobject Obj { get; set; }
        private int MaxAuthors { get; set; }

        public JsonFormatter(string evalModel, int maxAuthors = 3)
        {
            try
            {
                this.Obj = JsonConvert.DeserializeObject<EvaluateModel.Rootobject>(evalModel);
            }
            catch(Exception e)
            {
                // Do nothing
            }
            this.MaxAuthors = maxAuthors;
        }
        public string FormatEvaluateModel()
        {
            return HandleEntities(Obj.entities);
        }

        private string HandleEntities(EvaluateModel.Entity[] entities)
        {
            if(entities == null)
            {
                return string.Empty;
            }
            List<string> entityMarkdownList = new List<string>();
            foreach(var e in entities)
            {
                entityMarkdownList.Add(HandleEntity(e));
            }
            return string.Join("\n\n\n\n", entityMarkdownList);
        }

        private string HandleEntity(EvaluateModel.Entity entity)
        {
            ExtendedAttributesModel.Rootobject extendedObj;
            extendedObj = JsonConvert.DeserializeObject<ExtendedAttributesModel.Rootobject>(entity.E);

            // Add title and authors
            string reply = string.Format("\"{0}\", {1}",
                            HandleTitle(extendedObj.DN, entity.Id),
                            HandleAuthors(entity.AA));

            // Add venue
            string venueStr = HandleVenue(entity, extendedObj);
            if (!String.IsNullOrEmpty(venueStr))
            {
                reply += String.Format(", {0}", venueStr);
            }

            // Add year
            reply += String.Format(" {0}", HandleYear(entity.Y));
            
            // Add FOS
            string fosStr = HandleFieldOfStudies(entity.F);
            if(!String.IsNullOrEmpty(fosStr))
            {
                reply += String.Format(" (Topics: {0})", fosStr);
            }

            return reply;
        }

        private string HandleVenue(EvaluateModel.Entity entity,
                                   ExtendedAttributesModel.Rootobject extendedObj)
        {
            string venueDisplayName = string.Empty;
            long venueId = 0; 
            if(!String.IsNullOrEmpty(extendedObj.VSN))
            {
                venueDisplayName = extendedObj.VSN;
            }
            else if (!String.IsNullOrEmpty(extendedObj.VFN))
            {
                venueDisplayName = extendedObj.VFN;
            }

            if((entity.C != null) && !String.IsNullOrEmpty(entity.C.CN))
            {
                venueId = entity.C.CId;
            }
            else if ((entity.J != null) && !String.IsNullOrEmpty(entity.J.JN))
            {
                venueId = entity.J.JId;
            }

            if (!String.IsNullOrEmpty(venueDisplayName))
            {
                return string.Format("[{0}]{1}", venueDisplayName, CreateEntityLink(venueId));
            }
            return string.Empty;
        }

        private string HandleFieldOfStudies(EvaluateModel.F[] fosList)
        {
            if(fosList == null)
            {
                return string.Empty;
            }
            List<string> fosStrList = new List<string>();
            foreach(var f in fosList)
            {
                fosStrList.Add(HandleFos(f));
            }
            return String.Join(", ", fosStrList);
        }

        private string HandleFos(EvaluateModel.F fos)
        {
            return string.Format("[{0}]{1}", fos.FN, CreateEntityLink(fos.FId));
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
            if(authors == null)
            {
                return string.Empty;
            }
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
