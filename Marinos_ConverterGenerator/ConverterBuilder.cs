using System.Collections.Generic;

using Microsoft.Practices.Prism.ViewModel;

namespace Marinos_ConverterGenerator
{
    public class ConverterBuilder : NotificationObject
    {
        private MWVM   VM;
        private string _res;

        public ConverterBuilder(MWVM vm)
        {
            VM              = vm;
        }

        public void Add(string str)
        {
            _res += str;
        }

        public void AddLine(string str)
        {
            _res += $"{str}\n";
        }

        public void AddLine()
        {
            _res += $"\n";
        }

        public string GetToConverterResult()
        {
            if (!VM.ExportToCompany && !VM.ExportToShip)
                return null;

            _res =  "";
            AddLine($"public class {VM.EntityName}ToSerializeConverter : ");

            if (VM.ExportToCompany && VM.ExportToShip)
                Add("\t\tManagedByTwoEntityToSerializeConverter");
            else if (VM.ExportToCompany)
                Add("\t\tManagedByCompanyEntityToSerializeConverter");
            else
                Add("\t\tManagedByShipEntityToSerializeConverter");

            Add("<");
            AddLine($"{VM.EntityName}, Serializable{VM.EntityName}>,");
            AddLine($"\t\tIToConverter");
            AddLine("{");

            AddLine($"\tpublic {VM.EntityName}ToSerializeConverter() {{ }}");
            AddLine();
            #region Package_ListForAdd

            AddLine($"\tprotected override List<Serializable{VM.EntityName}> Package_ListForAdd");
            AddLine("\t{");
            AddLine($"\t\tget => return Package.{VM.EntityName}ToAdd;");
            AddLine($"\t\tset => Package.{VM.EntityName}ToAdd = value;");
            AddLine("\t}");
            AddLine();
            
            #endregion

            #region Package_ListForEdit
            
            AddLine($"\tprotected override List<Serializable{VM.EntityName}> Package_ListForEdit");
            AddLine("\t{");
            AddLine($"\t\tget => return Package.{VM.EntityName}ToModify;");
            AddLine($"\t\tset => Package.{VM.EntityName}ToModify = value;");
            AddLine("\t}");
            AddLine();

            #endregion

            #region SetFields

            AddLine($"\tprotected override void SetFields(Serializable{VM.EntityName} serializableEntity, {VM.EntityName} entity)");
            AddLine("\t{");

            foreach (var item in VM.Properties)
                AddLine($"\t\tserializableEntity.{item.Name} = entity.{item.Name};");

            if (VM.FK_Entities.Count != 0)
            {
                AddLine();
                AddLine("\t\t//переворациваем Id* и IDOuter при экспорте");
                AddLine();
            }

            foreach (var item in VM.FK_Entities)
            {
                if (item.Nullable)
                {
                    AddLine($"\t\tif(entity.Id{item.NavigationPropertyName} != null)");
                    AddLine("\t\t{");
                }

                AddLine($"{(item.Nullable ? "\t" : "")}\t\tserializableEntity.Id{item.NavigationPropertyName}Outer = entity.Id{item.NavigationPropertyName};");
                AddLine($"{(item.Nullable ? "\t" : "")}\t\tserializableEntity.Id{item.NavigationPropertyName} = entity.{item.NavigationPropertyName}.IDOuter;");
                
                if(item.Nullable)
                    AddLine("\t\t}");
                AddLine();
            }

            #endregion

            #region ConverterSpecificity

            AddLine("\tprotected override ConverterSpecificity ConverterSpecificity =>");
            AddLine("\t\t\tnew ConverterSpecificity(exportToShip: true,");
            AddLine("\t\t\t\t\t\t\t\t\t exportToCompany: false,");
            AddLine("\t\t\t\t\t\t\t\t\t isOwnedByShip: true,");
            AddLine("\t\t\t\t\t\t\t\t\t projects: new List<BaseLib.CommonProgrammInfo.ProjectName>()");
            AddLine("\t\t\t\t\t\t\t\t\t {");
            AddLine("\t\t\t\t\t\t\t\t\t \t\tBaseLib.CommonProgrammInfo.ProjectName.MARINOS");
            AddLine("\t\t\t\t\t\t\t\t\t });");
            #endregion

            #region Close class

            AddLine("}");

            #endregion
            return _res;
        }
    }
    
    
}