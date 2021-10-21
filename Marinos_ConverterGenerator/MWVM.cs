using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Marinos_ConverterGenerator
{
    [Serializable]
    public class MWVM : NotificationObject
    {

        #region private properties

        private string                         _entityName;
        private bool                           _exportToShip;
        private bool                           _exportToCompany;
        private ObservableCollection<FKEntity> _fkEntities;
        private FKEntity                       _selectedFkEntity;
        private FKEntity                       _newFkEntity;
        private ConverterBuilder               _builder;
        private ObservableCollection<Property> _properties;
        private Property                       _selectedProperty;
        private Property                       _newProperty;
        private bool                           _isOwnedByShip;
        private bool                           _isTree;
        private string                         _parentName;
        private string                         _resultConverter;
        private string                         _resultSeriazableEntity;
        private string                         _resultLoader;
        private string                         _resultEntity;
        private string                         _resultSeriazablePackage;
        private string                         _selectedPropertyType;
        private string                         _smallEntityName;

        #endregion

        #region public properties

        public string ParentName
        {
            get => _parentName;
            set
            {
                _parentName = value;
                GetResults();
                RaisePropertyChanged(() => ParentName);
                SaveToXml();
            }
        }

        public string EntityName
        {
            get => _entityName;
            set
            {
                _entityName = value;
                GetResults();
                RaisePropertyChanged(() => EntityName);
                SaveToXml();
            }
        }

        public string SmallEntityName
        {
            get => _smallEntityName;
            set
            {
                _smallEntityName = value; 
                GetResults();
                RaisePropertyChanged(() => SmallEntityName);
                SaveToXml();
            }
        }

        public bool ExportToShip
        {
            get => _exportToShip;
            set
            {
                _exportToShip = value;
                if (_exportToCompany && !_exportToShip)
                    IsOwnedByShip = true;

                GetResults();
                RaisePropertyChanged(() => ExportToShip);
                SaveToXml();
            }
        }

        public bool ExportToCompany
        {
            get => _exportToCompany;
            set
            {
                _exportToCompany = value;
                if (_exportToCompany && !_exportToShip)
                    IsOwnedByShip = true;

                GetResults();
                RaisePropertyChanged(() => ExportToCompany);
                SaveToXml();
            }
        }

        public bool IsOwnedByShip
        {
            get => _isOwnedByShip;
            set
            {
                _isOwnedByShip = value;
                GetResults();
                RaisePropertyChanged(() => IsOwnedByShip);
                SaveToXml();
            }
        }

        public bool IsTree
        {
            get => _isTree;
            set
            {
                _isTree = value;
                GetResults();
                RaisePropertyChanged(() => IsTree);
                SaveToXml();
            }
        }

        public ObservableCollection<FKEntity> FK_Entities
        {
            get => _fkEntities;
            set
            {
                _fkEntities = value;
                GetResults();
                RaisePropertyChanged(() => FK_Entities);
            }
        }

        [XmlIgnore]
        public FKEntity SelectedFKEntity
        {
            get => _selectedFkEntity;
            set
            {
                _selectedFkEntity = value;
                NewFKEntity       = new FKEntity(_selectedFkEntity);
                RaisePropertyChanged(() => SelectedFKEntity);
                RaiseCanExecChanged();
            }
        }

        [XmlIgnore]
        public FKEntity NewFKEntity
        {
            get => _newFkEntity;
            set
            {
                _newFkEntity = value;
                RaisePropertyChanged(() => NewFKEntity);
            }
        }

        public ObservableCollection<Property> Properties
        {
            get => _properties;
            set
            {
                _properties = value;
                GetResults();
                RaisePropertyChanged(() => Properties);
            }
        }

        [XmlIgnore]
        public Property SelectedProperty
        {
            get => _selectedProperty;
            set
            {
                _selectedProperty = value;
                NewProperty       = new Property(_selectedProperty);
                RaisePropertyChanged(() => SelectedProperty);
                RaiseCanExecChanged();
            }
        }

        [XmlIgnore]
        public Property NewProperty
        {
            get => _newProperty;
            set
            {
                _newProperty = value;
                RaisePropertyChanged(() => NewProperty);
            }
        }

        [XmlIgnore]
        public ConverterBuilder Builder
        {
            get => _builder;
            set
            {
                _builder = value;
                RaisePropertyChanged(() => Builder);
            }
        }

        [XmlIgnore]
        public string ResultConverter
        {
            get => _resultConverter;
            set
            {
                _resultConverter = value;
                if (g.ConverterTextEditor != null)
                    g.ConverterTextEditor.Text = _resultConverter;
                RaisePropertyChanged(() => ResultConverter);
            }
        }

        [XmlIgnore]
        public string ResultSeriazableEntity
        {
            get => _resultSeriazableEntity;
            set
            {
                _resultSeriazableEntity = value;
                if (g.SeriazableEntityTextEditor != null)
                    g.SeriazableEntityTextEditor.Text = _resultSeriazableEntity;
                RaisePropertyChanged(() => ResultSeriazableEntity);
            }
        }

        [XmlIgnore]
        public string ResultLoader
        {
            get => _resultLoader;
            set
            {
                _resultLoader = value;
                if (g.LoaderTextEditor != null)
                    g.LoaderTextEditor.Text = _resultLoader;
                RaisePropertyChanged(() => ResultLoader);
            }
        }

        [XmlIgnore]
        public string ResultEntity
        {
            get => _resultLoader;
            set
            {
                _resultEntity = value;
                if (g.EntityTextEditor != null)
                    g.EntityTextEditor.Text = _resultEntity;
                RaisePropertyChanged(() => ResultEntity);
            }
        }

        [XmlIgnore]
        public string ResultSeriazablePackage
        {
            get => _resultLoader;
            set
            {
                _resultSeriazablePackage = value;
                if (g.SeriazablePackageTextEditor != null)
                    g.SeriazablePackageTextEditor.Text = _resultSeriazablePackage;
                RaisePropertyChanged(() => ResultSeriazablePackage);
            }
        }

        [XmlIgnore]
        public ObservableCollection<string> PropertyTypes { get; set; }

        #endregion

        #region commands

        public DelegateCommand AddNewFKEntityCommand         { get; }
        public DelegateCommand SaveFKEntityCommand           { get; }
        public DelegateCommand RemoveSelectedFKEntityCommand { get; }
        public DelegateCommand AddNewPropertyCommand         { get; }
        public DelegateCommand SavePropertyCommand           { get; }
        public DelegateCommand RemoveSelectedPropertyCommand { get; }
        public DelegateCommand ClearFKEntitiesCommand        { get; }
        public DelegateCommand ClearPropertiesCommand        { get; }
        public DelegateCommand TREEHALP                      { get; }

        #endregion

        #region ctor

        public MWVM()
        {
            AddNewFKEntityCommand         = new DelegateCommand(OnAddNewFKEntity);
            AddNewPropertyCommand         = new DelegateCommand(OnAddNewProperty);
            SaveFKEntityCommand           = new DelegateCommand(OnSaveFKEntity,           () => FK_Entities.Count(x => x.Id == NewFKEntity.Id) >= 0);
            SavePropertyCommand           = new DelegateCommand(OnSaveProperty,           () => Properties.Count(x => x.Id  == NewProperty.Id) >= 0);
            RemoveSelectedFKEntityCommand = new DelegateCommand(OnRemoveSelectedEntity,   () => FK_Entities.IndexOf(SelectedFKEntity)          >= 0);
            RemoveSelectedPropertyCommand = new DelegateCommand(OnRemoveSelectedProperty, () => Properties.IndexOf(SelectedProperty)           >= 0);
            ClearFKEntitiesCommand        = new DelegateCommand(OnClearFKEntities,        () => FK_Entities != null && FK_Entities.Count != 0);
            ClearPropertiesCommand        = new DelegateCommand(OnClearProperties,        () => Properties  != null && Properties.Count  != 0);
            TREEHALP                      = new DelegateCommand(OnTREEHALP);

            Builder       = new ConverterBuilder(this);
            NewFKEntity   = new FKEntity(0);
            NewProperty   = new Property(0);
            FK_Entities   = new ObservableCollection<FKEntity>();
            Properties    = new ObservableCollection<Property>();
            PropertyTypes = new ObservableCollection<string>(new List<string>
                                                             {
                                                                 "string",
                                                                 "int",
                                                                 "DateTime",
                                                                 "bool",
                                                                 "float",
                                                                 "double",
                                                                 "decimal",
                                                                 "short",
                                                                 "long"
                                                             });
        }

        #endregion

        #region private funcs

        private static object _lock = new object();
        private static bool   _isLoading;
        private void SaveToXml()
        {
            if (_isLoading) return;
            lock (_lock)
            {
                var formatter = new XmlSerializer(typeof(MWVM));
                using (var fs = new FileStream("CurrentState.xml", FileMode.Create))
                    formatter.Serialize(fs, this);
            }
        }

        public static MWVM LoadFromXml()
        {
            _isLoading = true;
            lock(_lock)
            {
                if (!File.Exists("CurrentState.xml")) return new MWVM();
                var formatter = new XmlSerializer(typeof(MWVM));

                using (var fs = new FileStream("CurrentState.xml", FileMode.Open))
                {
                    var item = (MWVM)formatter.Deserialize(fs);

                    _isLoading = false;
                    return item;
                }
            }

            _isLoading = false;
        }


        private void OnAddNewFKEntity()
        {
            if (NewFKEntity.Id == SelectedFKEntity?.Id)
                NewFKEntity = new FKEntity(FK_Entities.Count);

            if (string.IsNullOrEmpty(NewFKEntity.EntityName))
                return;

            if (string.IsNullOrEmpty(NewFKEntity.NavigationPropertyName))
                return;
            
            if (FK_Entities.Count(x => x.EntityName == NewFKEntity.EntityName) != 0)
            {
                MessageBox.Show("Сущность с таким названием уже добавлена!");
                return;
            }

            if (FK_Entities.Count(x => x.NavigationPropertyName == NewFKEntity.NavigationPropertyName) != 0)
            {
                MessageBox.Show("Сущность с таким названием нав. свойства уже добавлена!");
                return;
            }

            FK_Entities.Add(NewFKEntity);

            NewFKEntity = new FKEntity(FK_Entities.Count);
            GetResults();
            SaveToXml();
        }

        private void OnSaveFKEntity()
        {
            var entity = FK_Entities.FirstOrDefault(x => x.Id == NewFKEntity.Id);

            if (entity == null)
                OnAddNewFKEntity();
            else
            {
                var newEntity = new FKEntity(NewFKEntity);
                var ind       = FK_Entities.IndexOf(entity);
                FK_Entities.RemoveAt(ind);
                FK_Entities.Insert(ind, newEntity);

                for (var i = 0; i < FK_Entities.Count; i++)
                    FK_Entities[i].Id = i;

                SelectedFKEntity = newEntity;
                GetResults();
                SaveToXml();
            }
        }

        private void OnRemoveSelectedEntity()
        {
            FK_Entities.Remove(SelectedFKEntity);
            SelectedFKEntity = null;

            for (var i = 0; i < FK_Entities.Count; i++)
                FK_Entities[i].Id = i;

            GetResults();
            SaveToXml();
        }

        private void OnClearFKEntities()
        {
            FK_Entities.Clear();
            SelectedFKEntity = null;

            GetResults();
            SaveToXml();
        }

        private void RaiseCanExecChanged()
        {
            AddNewFKEntityCommand.RaiseCanExecuteChanged();
            SaveFKEntityCommand.RaiseCanExecuteChanged();
            RemoveSelectedFKEntityCommand.RaiseCanExecuteChanged();
            AddNewPropertyCommand.RaiseCanExecuteChanged();
            SavePropertyCommand.RaiseCanExecuteChanged();
            RemoveSelectedPropertyCommand.RaiseCanExecuteChanged();
            SaveToXml();
        }

        private void OnAddNewProperty()
        {
            if (NewProperty.Id == SelectedProperty?.Id)
                NewProperty = new Property(Properties.Count);

            if (string.IsNullOrEmpty(NewProperty.Name))
                return;

            if (string.IsNullOrEmpty(NewProperty.Type))
            {
                MessageBox.Show("Выберите тип");
                return;
            }

            if (Properties.Count(x => x.Name == NewProperty.Name) != 0)
            {
                MessageBox.Show("Уже есть");
                return;
            }

            Properties.Add(NewProperty);

            NewProperty = new Property(Properties.Count);
            GetResults();
            SaveToXml();
        }

        private void OnSaveProperty()
        {
            var property = Properties.FirstOrDefault(x => x.Id == NewProperty.Id);

            if (property == null)
                OnAddNewProperty();
            else
            {
                var newProperty = new Property(NewProperty);
                var ind         = Properties.IndexOf(property);
                Properties.RemoveAt(ind);
                Properties.Insert(ind, newProperty);

                for (var i = 0; i < Properties.Count; i++)
                    Properties[i].Id = i;

                SelectedProperty = newProperty;
                GetResults();
                SaveToXml();
            }
        }

        private void OnRemoveSelectedProperty()
        {
            Properties.Remove(SelectedProperty);
            SelectedProperty = null;

            for (var i = 0; i < Properties.Count; i++)
                Properties[i].Id = i;

            GetResults();
            SaveToXml();
        }

        private void OnClearProperties()
        {
            Properties.Clear();
            SelectedProperty = null;

            GetResults();
            SaveToXml();
        }

        private void OnTREEHALP()
        {
            MessageBox.Show("Очень кривая реализация экспорта / импорта для дерева.\n"          +
                            "В таблице сущности должно быть поле ParentId и ParentIDOuter!!!\n" +
                            "(пример смотри в СУБе ImportExport\\Managers\\Converters\\ManagedByCompany\\DMS_SmsPartitionToSerializeConvertor.cs)");
        }

        #endregion

        #region public funcs

        public void GetResults()
        {
            ResultConverter         = Builder.GetConverterResult();
            ResultSeriazableEntity  = Builder.GetSeriazableEntityResult();
            ResultLoader            = Builder.GetLoaderResult();
            ResultEntity            = Builder.GetEntityResult();
            ResultSeriazablePackage = Builder.GetSeriazablePackageResult();
        }

        #endregion
    }
    [ValueConversion(typeof(double), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value,     Type        targetType,
                              object parameter, CultureInfo culture)
        {
            var res = Visibility.Collapsed;
            if (value == null)
                return res;

            var val = (bool)value;
            res = !val ? Visibility.Collapsed : Visibility.Visible;
            return res;
        }

        public object ConvertBack(object value,     Type        targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}