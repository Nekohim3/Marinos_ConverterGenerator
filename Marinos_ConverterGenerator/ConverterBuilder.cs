using System.Collections.Generic;
using System.Linq;

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

            #region ToConverter
            AddLine($"public class {VM.EntityName}ToSerializeConverter : ");

            if (VM.ExportToCompany && VM.ExportToShip)
                Add("\t\tManagedByTwoEntityToSerializeConverter<");
            else if (VM.ExportToCompany)
                Add("\t\tManagedByCompanyEntityToSerializeConverter<");
            else
                Add("\t\tManagedByShipEntityToSerializeConverter<");

            AddLine($"{VM.EntityName}, Serializable{VM.EntityName}>,");
            AddLine($"\t\tIToConverter");
            AddLine("{");

            AddLine($"\tpublic {VM.EntityName}ToSerializeConverter() {{ }}");
            AddLine();
            
            #region Package_ListForAdd

            AddLine($"\tprotected override List<Serializable{VM.EntityName}> Package_ListForAdd");
            AddLine("\t{");
            AddLine($"\t\tget => Package.{VM.EntityName}ToAdd;");
            AddLine($"\t\tset => Package.{VM.EntityName}ToAdd = value;");
            AddLine("\t}");
            AddLine();
            
            #endregion

            #region Package_ListForEdit
            
            AddLine($"\tprotected override List<Serializable{VM.EntityName}> Package_ListForEdit");
            AddLine("\t{");
            AddLine($"\t\tget => Package.{VM.EntityName}ToModify;");
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
            
            AddLine("\t}");
            AddLine();

            #endregion

            #region ConverterSpecificity

            AddLine("\tprotected override ConverterSpecificity ConverterSpecificity =>");
            AddLine($"\t\t\tnew ConverterSpecificity(exportToShip: {VM.ExportToShip},");
            AddLine($"\t\t\t\t\t\t\t\t\t exportToCompany: {VM.ExportToCompany},");
            AddLine($"\t\t\t\t\t\t\t\t\t isOwnedByShip: {VM.IsOwnedByShip},");
            AddLine("\t\t\t\t\t\t\t\t\t projects: new List<BaseLib.CommonProgrammInfo.ProjectName>()");
            AddLine("\t\t\t\t\t\t\t\t\t {");
            AddLine("\t\t\t\t\t\t\t\t\t \t\tBaseLib.CommonProgrammInfo.ProjectName.MARINOS");
            AddLine("\t\t\t\t\t\t\t\t\t });");
            #endregion

            #region Close class

            AddLine("}");

            #endregion
            
            AddLine();
            
            #endregion


            #region FromConverter

            AddLine($"public class {VM.EntityName}FromSerializeConverter : ");

            if (VM.ExportToCompany && VM.ExportToShip)
                Add("\t\tManagedByTwoEntityToSerializeConverter<");
            else if (VM.ExportToCompany)
                Add("\t\tManagedByCompanyEntityToSerializeConverter<");
            else
                Add("\t\tManagedByShipEntityToSerializeConverter<");

            AddLine($"Serializable{VM.EntityName}, {VM.EntityName}>,");
            AddLine($"\t\tIFromConverter");
            AddLine("{");

            AddLine($"\tpublic {VM.EntityName}FromSerializeConverter() {{ }}");
            AddLine();
            
            #region Package_ListForAdd

            AddLine($"\tprotected override List<Serializable{VM.EntityName}> AddedList =>");
            AddLine($"\t\tPackage.{VM.EntityName}ToAdd.ToList();");
            AddLine();
            
            #endregion

            #region Package_ListForEdit
            
            AddLine($"\tprotected override List<Serializable{VM.EntityName}> ModifiedList =>");
            AddLine($"\t\tPackage.{VM.EntityName}ToModify.ToList();");
            AddLine();

            #endregion

            #region Create entity

            AddLine($"\tprotected override {VM.EntityName} CreateDbEntity() =>");
            AddLine($"\t\t\tnew {VM.EntityName}();");
            AddLine();

            #endregion
            
            #region check complex

            var reqList   = VM.FK_Entities.Where(x => !x.Nullable).ToList();
            var noReqList = VM.FK_Entities.Where(x => x.Nullable).ToList();

            #region Req

            if (reqList.Count != 0)
            {
                AddLine($"\t// ТУТ МОЖЕТ ПОНАДОБИТЬСЯ ДОПОЛНИТЕЛЬНАЯ ЛОГИКА. ПИСАТЬ ВРУЧНУЮ!!!");
                AddLine($"\tprotected override bool CheckRequiredComplexProperty(Serializable{VM.EntityName} serializeEntity, {VM.EntityName} entity)");
                AddLine("\t{");
                AddLine($"\t\tvar complexFieldHelper = new ComplexFieldHelper(DbImportManager);");
            }
            
            foreach (var item in reqList)
            {
                AddLine($"\t\tvar id{item.NavigationPropertyName} = entity.Id{item.NavigationPropertyName} == 0 ? null : entity.Id{item.NavigationPropertyName};");
                AddLine($"\t\tvar set{item.NavigationPropertyName} = false");
                AddLine($"\t\tif(complexFieldHelper.CheckComplexField<{item.EntityName}>(ref id{item.NavigationPropertyName}, out set{item.NavigationPropertyName}, serializeEntity.Id{item.NavigationPropertyName}, serializeEntity.Id{item.NavigationPropertyName}Outer))");
                AddLine("\t\t\treturn false;");
                AddLine();
                AddLine($"\t\tif(set{item.NavigationPropertyName})");
                AddLine($"\t\t\tentity.Id{item.NavigationPropertyName} = id{item.NavigationPropertyName}.Value");
                AddLine();
            }
            
            if (reqList.Count != 0)
            {
                AddLine("\t}");
                AddLine();
            }
            
            #endregion

            #region noReq

            if (noReqList.Count != 0)
            {
                AddLine($"\t// ТУТ МОЖЕТ ПОНАДОБИТЬСЯ ДОПОЛНИТЕЛЬНАЯ ЛОГИКА. ПИСАТЬ ВРУЧНУЮ!!!");
                AddLine($"\tprotected override bool CheckAndSetOptionalComplexProperty(Serializable{VM.EntityName} serializeEntity, {VM.EntityName} entity)");
                AddLine("\t{");
                AddLine($"\t\tvar complexFieldHelper = new ComplexFieldHelper(DbImportManager);");
            }

            for (var i = 0; i < noReqList.Count; i++)
            {
                var item = noReqList[i];
                AddLine($"\t\tvar needChangeSelfModifiedDate = false;");
                AddLine(
                    $"\t\tvar id{item.NavigationPropertyName} = entity.Id{item.NavigationPropertyName} == 0 ? null : entity.Id{item.NavigationPropertyName};");
                AddLine($"\t\tvar set{item.NavigationPropertyName} = false");
                AddLine(
                    $"\t\tneedChangeSelfModifiedDate = {(i > 0 ? "needChangeSelfModifiedDate | " : "")}complexFieldHelper.CheckComplexField<{item.EntityName}>(ref id{item.NavigationPropertyName}, out set{item.NavigationPropertyName}, serializeEntity.Id{item.NavigationPropertyName}, serializeEntity.Id{item.NavigationPropertyName}Outer))");
                AddLine();
                AddLine($"\t\tif(set{item.NavigationPropertyName})");
                AddLine($"\t\t\tentity.Id{item.NavigationPropertyName} = id{item.NavigationPropertyName}.Value");
                AddLine($"\t\treturn needChangeSelfModifiedDate;");
            }

            if (noReqList.Count != 0)
            {
                AddLine("\t}");
                AddLine();
            }
            
            #endregion
            
            #endregion

            #region ImportAtCompany

            if (VM.ExportToCompany)
            {
                AddLine($"\tprotected override void ImportAtCompany(Serializable{VM.EntityName} serializeEntity, {VM.EntityName} entity)");
                AddLine("\t{");

                foreach (var item in VM.Properties)
                    AddLine($"\t\tentity.{item.Name} = serializeEntity.{item.Name};");

                AddLine("\t}");
                AddLine();
            }

            #endregion

            #region ImportAtShip

            if (VM.ExportToShip)
            {
                AddLine($"\tprotected override void ImportAtShip(Serializable{VM.EntityName} serializeEntity, {VM.EntityName} entity)");
                AddLine("\t{");

                foreach (var item in VM.Properties)
                    AddLine($"\t\tentity.{item.Name} = serializeEntity.{item.Name};");

                AddLine("\t}");
            }

            #endregion

            #region CloseClass

            AddLine("}");

            #endregion
            #endregion
            return _res;
        }
    }
    
    
}