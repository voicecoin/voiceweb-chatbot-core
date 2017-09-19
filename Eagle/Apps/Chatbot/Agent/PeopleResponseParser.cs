﻿using Apps.Chatbot.Entity;
using Apps.Chatbot.Intent;
using Apps.Nlp;
using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Apps.Chatbot.Agent
{
    public class PeopleResponseParser
    {
        public string FillReplyTemplate(CoreDbContext Dc, List<IntentResponseParameterEntity> parameterEntities, string template)
        {
            string response = template;

            // 提取函数
            MatchCollection mcFunc = Regex.Matches(template, @"=ValidateProperty\(.+,.+,.+\)");
            foreach (Match m in mcFunc)
            {
                response = "不知道呀。";
                string[] parameters = m.Value.Substring(18, m.Value.Length - 19).Split(',');
                string subject = parameters.First();
                string obj = parameters.Last();

                string entityValue = parameters[1];
                string entityName = parameterEntities.First(x => x.Value.ToString() == entityValue).Value;

                List<Triple> triples = CnDbPeidaRdf.QueryEntity(Dc, subject);
                /*List<String> synonoms = aiController.GetAllSynonoms(entityName, entityValue);

                foreach (String synonym in synonoms)
                {
                    Triple triple = triples.FirstOrDefault(x => (x.Predicate as LiteralNode).Value.Contains(synonym));

                    if (triple != null)
                    {
                        string objValue = (triple.Object as LiteralNode).Value;
                        response = objValue == obj ? "是的。" : "不是。";
                        break;
                    }
                }*/
            }

            // 提取函数
            MatchCollection mcFunc2 = Regex.Matches(template, @"=ExistsProperty\(.+,.+\)");
            foreach (Match m in mcFunc2)
            {
                response = "不知道呀。";
                string[] parameters = m.Value.Substring(16, m.Value.Length - 17).Split(',');
                string subject = parameters.First();

                string entityValue = parameters.Last();
                string entityName = parameterEntities.First(x => x.Value.ToString() == entityValue).Value;

                List<Triple> triples = CnDbPeidaRdf.QueryEntity(Dc, subject);
                List<String> synonoms = (from entity in Dc.Table<EntityEntity>()
                                        join entry in Dc.Table<EntityEntryEntity>() on entity.Id equals entry.EntityId
                                        join synonym in Dc.Table<EntityEntrySynonymEntity>() on entry.Id equals synonym.EntityEntryId
                                        where entity.Id == "3286b09e-f542-42ec-a404-55b5d93e57bc" && entry.Value == entityValue
                                        select synonym.Synonym).ToList();

                foreach (String synonym in synonoms)
                {
                    Triple triple = triples.FirstOrDefault(x => x.Predicate.Contains(synonym));

                    if (triple != null)
                    {
                        response = "有。";
                        break;
                    }
                }
            }

            // 优先提取间接属性
            MatchCollection mc1 = Regex.Matches(template, "(@[A-Za-z0-9.]+)+");
            foreach (Match m in mc1)
            {
                string[] properties = m.Value.Split('.');

                response = TranslateToken(Dc, parameterEntities, template, properties.ToList());

                string tokenName = response.Split('.').Last();
                response = tokenName;
            }

            return response;
        }

        private String TranslateToken(CoreDbContext Dc, List<IntentResponseParameterEntity> parameterEntities, string template, List<String> tokens)
        {
            if (tokens.Count < 2) return template;

            template = TranslateToken(Dc, parameterEntities, template, tokens[0], tokens[1]);

            tokens.RemoveAt(0);

            template = TranslateToken(Dc, parameterEntities, template, tokens);

            return template;
        }

        private String TranslateToken(CoreDbContext Dc, List<IntentResponseParameterEntity> parameterEntities, string template, string subject, string predict)
        {
            // 查询知识库
            string subjectEntity = subject.Substring(1);
            string subjectValue = parameterEntities.First(x => x.Name == subjectEntity).Value;

            List<Triple> triples = CnDbPeidaRdf.QueryEntity(Dc, subjectValue);
            List<String> predicts = triples.Select(x => x.Predicate).ToList();

            string predictEntity = predict.Substring(1);
            string predictValue = parameterEntities.First(x => x.Name == predictEntity).Value;

            string objectValue = String.Empty;

            List<String> synonoms = (from entity in Dc.Table<EntityEntity>()
                                     join entry in Dc.Table<EntityEntryEntity>() on entity.Id equals entry.EntityId
                                     join synonym in Dc.Table<EntityEntrySynonymEntity>() on entry.Id equals synonym.EntityEntryId
                                     where entity.Id == "3286b09e-f542-42ec-a404-55b5d93e57bc" && (entry.Value == predictValue || synonym.Synonym == predictValue)
                                     select synonym.Synonym).ToList();

            foreach (String synonym in synonoms)
            {
                Triple triple = triples.FirstOrDefault(x => x.Predicate.Contains(synonym));

                if (triple != null)
                {
                    objectValue = triple.Object;
                    objectValue = objectValue.Length > 128 ? objectValue.Split('。').FirstOrDefault() : objectValue;

                    //outputContext.Parameters[predict.Substring(1)] = objectValue;

                    template = template.Replace(predict, objectValue);
                    break;
                }
            }

            // 没找到属性值
            if (String.IsNullOrEmpty(objectValue))
            {
                //outputContext.Parameters[predict.Substring(1)] = "不知道，或许度娘知道。";
            }

            template = template.Replace(subject, subjectValue);

            return template;
        }
    }
}