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

        public string GetConverterResult()
        {
            if (!VM.ExportToCompany && !VM.ExportToShip)
                return null;

            _res =  "";
            AddLine("//MARINER/ImportExport/Managers/Converters");
            AddLine();

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

            if (VM.IsTree)
            {
                AddLine($"\tprivate List<Serializable{VM.EntityName}> _addedNotHierarchicallyBuiltChilds;");
                AddLine();
            }
            
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
            AddLine();

            if (VM.FK_Entities.Count != 0)
            {
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

            if (VM.IsTree)
            {
                AddLine("\t\tif (entity.IdParent != null)");
                AddLine("\t\t{");
                AddLine($"\t\t\tserializableEntity.IdParentOuter = entity.IdParent;");
                AddLine($"\t\t\tserializableEntity.IdParent = entity.{VM.ParentName}.IDOuter;");
                AddLine("\t\t}");
                AddLine();
                AddLine($"\t\tserializableEntity.Childs = new List<Serializable{VM.EntityName}>();");
            }
            
            AddLine("\t}");
            AddLine();

            #endregion

            if (VM.IsTree)
            {
                AddLine($"\tprotected override void MoveAddedEntityLists(List<Serializable{VM.EntityName}> listForAdd)");
                AddLine("\t{");
                AddLine("\t\tif (listForAdd != null && listForAdd.Count > 0)");
                AddLine("\t\t{");
                AddLine("\t\t\tif (_addedNotHierarchicallyBuiltChilds == null)");
                AddLine("\t\t\t\t_addedNotHierarchicallyBuiltChilds = listForAdd;");
                AddLine("\t\t\telse");
                AddLine("\t\t\t\t_addedNotHierarchicallyBuiltChilds.AddRange(listForAdd);");
                AddLine("\t\t}");
                AddLine("\t}");
                AddLine();
                AddLine("\t//переопределяем кол-во элементов, т.к. упорядочили их в виде дерева и Count() выдаст не верную информацию");
                AddLine($"\tpublic override int GetCount_Add(SerializablePackage package) => package.NumberOf{VM.EntityName}ToAdd;");
                AddLine();
                AddLine($"\tpublic override int GetCount_Edit(SerializablePackage package) => package.NumberOf{VM.EntityName}ToEdit;");
                AddLine();

                AddLine("\tprotected override void FinishActions()");
                AddLine("\t{");
                AddLine("\t\t//запоминаем в пакет кол-во для добавления и редактирования");
                AddLine($"\t\tPackage.NumberOf{VM.EntityName}ToAdd  = _addedNotHierarchicallyBuiltChilds?.Count() ?? 0;");
                AddLine($"\t\tPackage.NumberOf{VM.EntityName}ToEdit = Package_ListForEdit.Count;");
                AddLine("\t\t//строим ветви для добавленных");
                AddLine($"\t\tPackage_ListForAdd = GetHierarchicallyBuilt(_addedNotHierarchicallyBuiltChilds);");
                AddLine("\t}");
                AddLine();

                AddLine($"\tprivate static List<Serializable{VM.EntityName}> GetHierarchicallyBuilt(List<Serializable{VM.EntityName}> listToAdd)");
                AddLine("\t{");
                AddLine($"\t\tvar branches = new List<Serializable{VM.EntityName}>();");
                AddLine();
                AddLine($"\t\tif (listToAdd == null || listToAdd.Count() == 0) return branches");
                AddLine();
                AddLine($"\t\tforeach (var section in listToAdd)");
                AddLine("\t\t{");
                AddLine($"\t\t\tSerializable{VM.EntityName} parent = null;");
                AddLine();
                AddLine($"\t\t\tif (section.IdParentOuter != null)");
                AddLine($"\t\t\t\tparent = listToAdd.Find(item => item.IdOuter == section.IdParentOuter);");
                AddLine();
                AddLine($"\t\t\tif (parent != null)");
                AddLine("\t\t\t\tparent.Childs.Add(section);");
                AddLine("\t\t\telse");
                AddLine("\t\t\t\tbranches.Add(section);");
                AddLine("\t\t}");
                AddLine();
                AddLine("\t\treturn branches;");
                AddLine("\t}");
                AddLine();

            }

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

            if (VM.IsTree)
            {
                AddLine($"\tprivate TreeListPaginator<Serializable{VM.EntityName}> _treeListPaginator;");
                AddLine();
            }
            
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

            #region ForTree

            if (VM.IsTree)
            {
                AddLine($"\tprivate {VM.EntityName} FormAddedEntityWithChildren(Serializable{VM.EntityName} serializeEntity, {VM.EntityName} parent)");
                AddLine("\t{");
                AddLine($"\t\tvar entity = base.FormAddedEntity(serializeEntity, parent);");
                AddLine();
                AddLine("\t\tif (entity == null) return null;");
                AddLine();
                AddLine($"\t\tentity.Childs = new List<{VM.EntityName}>();");
                AddLine();
                AddLine("\t\tforeach (var item in serializeEntity.Childs)");
                AddLine("\t\t{");
                AddLine("\t\t\tvar childs = FormAddedEntityWithChildren(item, entity);");
                AddLine("\t\t\tif (childs != null) entity.Childs.Add(childs);");
                AddLine("\t\t}");
                AddLine();
                AddLine("\t\treturn entity;");
                AddLine("\t}");
                AddLine();

                AddLine($"\tprotected override {VM.EntityName} FormAddedEntityInListForAdding(Serializable{VM.EntityName} item) =>");
                AddLine("\t\tFormAddedEntityWithChildren(item, null);");
                AddLine();

                AddLine($"\tprotected override void StartSkipActionsForAdded(List<Serializable{VM.EntityName}> list, int skipNumber) =>");
                AddLine($"\t\t_treeListPaginator = new TreeListPaginator<Serializable{VM.EntityName}>(list, ImportArgs.PageLength, skipNumber, GetChildren);");
                AddLine();

                AddLine($"\tprotected override List<Serializable{VM.EntityName}> GetPage_Added(int skipNumber, int number, out int actualNumberInPage) =>");
                AddLine($"\t\t_treeListPaginator.GetPage(out actualNumberInPage);");
                AddLine();

                AddLine($"\tprivate List<Serializable{VM.EntityName}> GetChildren(Serializable{VM.EntityName} entity) => entity.Childs;");
                AddLine();
            }

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
                AddLine("\t\treturn true");
                AddLine("\t}");
                AddLine();
            }
            
            #endregion

            #region noReq

            if (noReqList.Count != 0 || VM.IsTree)
            {
                AddLine($"\t// ТУТ МОЖЕТ ПОНАДОБИТЬСЯ ДОПОЛНИТЕЛЬНАЯ ЛОГИКА. ПИСАТЬ ВРУЧНУЮ!!!");
                AddLine($"\tprotected override bool CheckAndSetOptionalComplexProperty(Serializable{VM.EntityName} serializeEntity, {VM.EntityName} entity)");
                AddLine("\t{");
                AddLine($"\t\tvar complexFieldHelper = new ComplexFieldHelper(DbImportManager);");
                AddLine($"\t\tvar needChangeSelfModifiedDate = false;");
                AddLine();
            }

            if (VM.IsTree)
            {
                AddLine(
                        $"\t\tvar idParent = entity.IdParent == 0 ? null : entity.IdParent;");
                AddLine($"\t\tvar setParent = false");
                AddLine(
                        $"\t\tneedChangeSelfModifiedDate = needChangeSelfModifiedDate | complexFieldHelper.CheckComplexField<{VM.EntityName}>(ref idParent, out setParent, serializeEntity.IdParent, serializeEntity.IdParentOuter))");
                AddLine();
                AddLine($"\t\tif(setParent)");
                AddLine($"\t\t\tentity.IdParent = idParent.Value");
                AddLine();
            }

            foreach (var item in noReqList)
            {
                AddLine(
                        $"\t\tvar id{item.NavigationPropertyName} = entity.Id{item.NavigationPropertyName} == 0 ? null : entity.Id{item.NavigationPropertyName};");
                AddLine($"\t\tvar set{item.NavigationPropertyName} = false");
                AddLine(
                        $"\t\tneedChangeSelfModifiedDate = needChangeSelfModifiedDate | complexFieldHelper.CheckComplexField<{item.EntityName}>(ref id{item.NavigationPropertyName}, out set{item.NavigationPropertyName}, serializeEntity.Id{item.NavigationPropertyName}, serializeEntity.Id{item.NavigationPropertyName}Outer))");
                AddLine();
                AddLine($"\t\tif(set{item.NavigationPropertyName})");
                AddLine($"\t\t\tentity.Id{item.NavigationPropertyName} = id{item.NavigationPropertyName}.Value");
                AddLine();
            }

            if (noReqList.Count != 0 || VM.IsTree)
            {
                AddLine($"\t\treturn needChangeSelfModifiedDate;");
                AddLine("\t}");
                AddLine();
            }

            #endregion

            #endregion

            #region AfterStepExecuted

            //if (VM.IsTree)
            //{
            //    AddLine("\tprotected override void AfterStepExecuted()");
            //    AddLine("\t{");
            //    AddLine($"\t\tvar parentList = {VM.EntityName}.GetAll();");
            //    AddLine();
            //    AddLine("\t\tforeach (var item in parentList)");
            //    AddLine("\t\t{");
            //    AddLine("\t\t\tif (item.ParentIDOuter != null && item.ParentId == null)");
            //    AddLine("\t\t\t{");
            //    AddLine("\t\t\t\tvar complexFieldHelper = new ComplexFieldHelper(DbImportManager);");
            //    AddLine($"\t\t\t\tvar entity = item;");
            //    AddLine("\t\t\t\tint? parentId = null;");
            //    AddLine("\t\t\t\tvar setParentId = false;");
            //    AddLine($"\t\t\t\tcomplexFieldHelper.CheckComplexField<{VM.EntityName}>(ref parentId, out setParentId, null, item.ParentIDOuter);");
            //    AddLine("\t\t\t\tif (setParentId)");
            //    AddLine("\t\t\t\t{");
            //    AddLine($"\t\t\t\t\tvar currentEntity = new {VM.EntityName}(entity.Id);");
            //    AddLine("\t\t\t\t\tcurrentEntity.ParentId = parentId.Value");
            //    AddLine("\t\t\t\t\tcurrentEntity.Save();");
            //    AddLine("\t\t\t\t}");
            //    AddLine("\t\t\t}");
            //    AddLine("\t\t}");
            //    AddLine("\t}");
            //    AddLine();
            //}

            #endregion

            #region ImportAtCompany

            if (VM.ExportToCompany)
            {
                AddLine($"\tprotected override void ImportAtCompany(Serializable{VM.EntityName} serializeEntity, {VM.EntityName} entity)");
                AddLine("\t{");

                foreach (var item in VM.Properties)
                    AddLine($"\t\tentity.{item.Name} = serializeEntity.{item.Name};");
                
                if (VM.IsTree)
                    AddLine("\t\tentity.ParentIDOuter = serializeEntity.ParentIdOuter;");

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

                if (VM.IsTree)
                    AddLine("\t\tentity.ParentIDOuter = serializeEntity.ParentIdOuter;");

                AddLine("\t}");
            }

            #endregion

            #region CloseClass

            AddLine("}");

            #endregion
            #endregion

            return _res;
        }

        public string GetSeriazableEntityResult()
        {
            if (!VM.ExportToCompany && !VM.ExportToShip)
                return null;

            _res = "";

            AddLine("//MARINER/ImportExport/SerializableEntities/DualChanged");
            AddLine("Создать класс");
            AddLine();

            AddLine($"public class Serializable{VM.EntityName} : DualChanged");
            AddLine("{");
            foreach (var item in VM.Properties)
                AddLine($"\tpublic {item.Type} {item.Name} {{ get; set; }}");

            if (VM.IsTree)
            {
                AddLine($"\tpublic int? IdParent {{ get; set; }}");
                AddLine($"\tpublic int? IdParentOuter {{ get; set; }}");
                AddLine($"\tpublic List<Serializable{VM.EntityName}> Childs {{ get; set; }}");
            }

            AddLine();
            AddLine($"\tSerializable{VM.EntityName}() {{ }}");
            AddLine("}");

            return _res;
        }

        public string GetLoaderResult()
        {
            if (!VM.ExportToCompany && !VM.ExportToShip)
                return null;

            _res = "";

            AddLine("//MARINER/DBServices/EI/EntityLoaders");
            AddLine("!!!Если надо лоадер файлов создаем вручную!!!");
            AddLine();
            
            AddLine("//GetConditionsFor... создать вручную если надо (Пример есть в лоадере заявок)");
            AddLine();

            AddLine($"class {VM.EntityName}Loader : {(VM.IsOwnedByShip ? "IEntityOwnedByShipLoader" : "IEntityOwnedByCompanyLoader")}<{VM.EntityName}, {VM.SmallEntityName}>");
            AddLine("{");

            AddLine($"\tprotected override IQueryable<{VM.SmallEntityName}> GetForExportToShip(MainContext context, EIArgs args) =>");
            AddLine($"\t\treturn context.{VM.SmallEntityName}.Include(ChangeInfoRecord.EntityName){(VM.FK_Entities.Count == 0 ? ";" : "")}");
            for (var i = 0; i < VM.FK_Entities.Count; i++)
            {
                var item = VM.FK_Entities[i];
                AddLine($"\t\t\t\t\t.Include({item.EntityName}).EntityName{(i == VM.FK_Entities.Count - 1 ? ";" : "")}");
            }
            AddLine();

            AddLine($"\tprotected override IQueryable<{VM.SmallEntityName}> GetForExportToCompany(MainContext context, EIArgs args) =>");
            AddLine($"\t\treturn context.{VM.SmallEntityName}.Include(ChangeInfoRecord.EntityName){(VM.FK_Entities.Count == 0 ? ";" : "")}");
            for (var i = 0; i < VM.FK_Entities.Count; i++)
            {
                var item = VM.FK_Entities[i];
                AddLine($"\t\t\t\t\t.Include({item.EntityName}).EntityName{(i == VM.FK_Entities.Count - 1 ? ";" : "")}");
            }
            AddLine();

            AddLine($"\tprotected override IQueryable<{VM.SmallEntityName}> GetAllForLoadingByIdOrIDOuter(MainContext context, bool loadWithReferences)");
            AddLine("\t{");
            AddLine($"\t\tSystem.Data.Entity.Infrastructure.DbQuery<{VM.SmallEntityName}> list = context.{VM.SmallEntityName};");
            AddLine("\t\tif (loadWithReferences)");
            AddLine($"\t\t\tlist = list.Include(ChangeInfoRecord.EntityName){(VM.FK_Entities.Count == 0 ? "; " : "")}");
            for (var i = 0; i < VM.FK_Entities.Count; i++)
            {
                var item = VM.FK_Entities[i];
                AddLine($"\t\t\t\t\t\t.Include({item.EntityName}).EntityName{(i == VM.FK_Entities.Count - 1 ? ";" : "")}");
            }
            AddLine("\t\treturn list;");
            AddLine("\t}");
            AddLine();

            AddLine($"\tprotected override {VM.EntityName} CreateEntity({VM.SmallEntityName} c_entity, EIArgs args) =>");
            AddLine($"\t\tnew {VM.EntityName}(c_entity);");
            AddLine();

            AddLine($"\tprotected override {VM.EntityName} CreateEntityLoadedByIdOrIDOuter({VM.SmallEntityName} c_entity, MainContext context) =>");
            AddLine($"\t\tnew {VM.EntityName}(c_entity, context);");
            AddLine();

            if (VM.IsTree)
            {
                AddLine($"\tprotected internal override void AddByImportInTransact({VM.EntityName} entity, ref List<UsedContext> listUsedContext)");
                AddLine("\t{");
                AddLine("\t\tentity.AddInTransact(ref listUsedContext);");
                AddLine();
                AddLine("\t\tif (entity.Childs == null) return;");
                AddLine("\t\tforeach (var item in entity.Childs)");
                AddLine("\t\t{");
                AddLine("\t\t\titem.ParentId = entity.Id;");
                AddLine("\t\t\tAddByImportInTransact(item, ref listUsedContext);");
                AddLine("\t\t}");
                AddLine("\t}");
                AddLine();

                AddLine($"\tprotected internal override void DeleteByRemoveCommandInTransact({VM.EntityName} entity, ref List<UsedContext> listUsedContext)");
                AddLine("\t{");
                AddLine("\t\tentity.LoadChilds();");
                AddLine("\t\tforeach (var item in entity.Childs) DeleteByRemoveCommandInTransact(item, ref listUsedContext);");
                AddLine("\t\tentity.DeleteInTransact(ref listUsedContext);");
                AddLine("\t}");
            }

            AddLine("}");

            return _res;
        }

        public string GetEntityResult()
        {
            if (!VM.ExportToCompany && !VM.ExportToShip)
                return null;

            _res = "";

            AddLine($"//в класс {VM.EntityName} добавить:");
            AddLine();
            if (VM.IsTree)
            {
                AddLine($"public List<{VM.EntityName}> Childs {{ get; set; }}");
                AddLine();
                AddLine("public void LoadChilds()");
                AddLine("{");
                AddLine($"\tChilds = new List<{VM.EntityName}>();");
                AddLine($"\tvar context = MainContext.CreateCurrentContext();");
                AddLine($"\tvar list = context.{VM.SmallEntityName}.Where(item => item.IdParent == Id);");
                AddLine();
                AddLine("\tforeach (var entity in list)");
                AddLine($"\t\tChilds.Add(new {VM.EntityName}(entity, context));");
                AddLine("}");
            }

            return _res;
        }

        public string GetSeriazablePackageResult()
        {
            if (!VM.ExportToCompany && !VM.ExportToShip)
                return null;

            _res = "";

            AddLine("//MARINER/ImportExport/SerializableEntities/Main/SerializablePackage.cs");
            if (VM.IsTree)
            {
                AddLine("В конец класса добавить:");
                AddLine();
                AddLine($"public int NumberOf{VM.EntityName}ToAdd  {{ get; set; }}");
                AddLine($"public int NumberOf{VM.EntityName}ToEdit {{ get; set; }}");
            }

            return _res;
        }

        public string GetAdditionalResult()
        {

            if (!VM.ExportToCompany && !VM.ExportToShip)
                return null;

            _res = "";
            
            AddLine("MARINER/DBServices/Base Entity/EntityTypeVariant.cs");
            AddLine($"В enum EntityTypeVariant добавить c уникальным числом (если небыло ранее добавлено в процессе создания {VM.EntityName}):");
            AddLine($"{VM.SmallEntityName} = число,");
            AddLine();
            AddLine($"Так же в {VM.EntityName} нужно добавить (если не сделано ранее):");
            AddLine($"public override EntityTypeVariant EntityType => EntityTypeVariant.{VM.SmallEntityName};");
            AddLine(new string('-', 50));
            
            AddLine("//MARINER/DBServices/EI/LoadersFactory.cs");
            AddLine("Добавить в конец функции GetEntityLoader() следующее:");
            AddLine();
            AddLine($"if (typeof(ChEntity) == typeof({VM.EntityName}))");
            AddLine($"\treturn new {VM.EntityName}Loader() as DBServices.EI.EntityLoaders.ILoaders.ILoader<ChEntity>;");
            AddLine();
            AddLine("Если создан лоадер файлов, то добавить его в конец функции GetEntityFileDataLoader()");
            AddLine(new string('-', 50));
            AddLine("//MARINER/ImportExport/Managers/Initialization/ImportStepSequence.cs");
            AddLine("Добавить в ImportStepEnum примерно следующее:");
            AddLine();
            AddLine("//Числа должны быть уникальными!!!");
            AddLine($"{VM.EntityName}_Add = число, {VM.EntityName}_Edit = число,");
            AddLine();
            AddLine("В GetOrderedSteps определить порядок:");
            AddLine();
            AddLine($"ImportStepEnum.{VM.EntityName}_Add,");
            AddLine($"ImportStepEnum.{VM.EntityName}_Edit,");
            AddLine("");
            AddLine("В GetOrderedCommands определить порядок удаления (нужно учитывать связи между таблицами в бд):");
            AddLine($"DBServices.EntityTypeVariant.{VM.SmallEntityName},");

            return _res;
        }
    }
    
    
}