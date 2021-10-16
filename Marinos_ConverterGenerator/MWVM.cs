using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace Marinos_ConverterGenerator
{
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
        private string                         _result;
        private ObservableCollection<Property> _properties;
        private Property                       _selectedProperty;
        private Property                       _newProperty;
        private bool                           _isOwnedByShip;

        #endregion

        #region public properties

        public string EntityName
        {
            get => _entityName;
            set
            {
                _entityName = value;
                Result      = Builder.GetToConverterResult();
                RaisePropertyChanged(() => EntityName);
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

                Result = Builder.GetToConverterResult();
                RaisePropertyChanged(() => ExportToShip);
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

                Result = Builder.GetToConverterResult();
                RaisePropertyChanged(() => ExportToCompany);
            }
        }

        public bool IsOwnedByShip
        {
            get => _isOwnedByShip;
            set { _isOwnedByShip = value; RaisePropertyChanged(() => IsOwnedByShip);}
        }

        public ObservableCollection<FKEntity> FK_Entities
        {
            get => _fkEntities;
            set
            {
                _fkEntities = value;
                Result      = Builder.GetToConverterResult();
                RaisePropertyChanged(() => FK_Entities);
            }
        }

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
                Result      = Builder.GetToConverterResult();
                RaisePropertyChanged(() => Properties);
            }
        }

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

        public Property NewProperty
        {
            get => _newProperty;
            set
            {
                _newProperty = value;
                RaisePropertyChanged(() => NewProperty);
            }
        }

        public ConverterBuilder Builder
        {
            get => _builder;
            set
            {
                _builder = value;
                RaisePropertyChanged(() => Builder);
            }
        }

        public string Result
        {
            get => _result;
            set
            {
                _result           = value;
                if(g.TextEditor != null)
                    g.TextEditor.Text = _result;
                RaisePropertyChanged(() => Result);
            }
        }

        #endregion

        #region commands

        public DelegateCommand AddNewFKEntityCommand         { get; }
        public DelegateCommand SaveFKEntityCommand           { get; }
        public DelegateCommand RemoveSelectedFKEntityCommand { get; }
        public DelegateCommand AddNewPropertyCommand         { get; }
        public DelegateCommand SavePropertyCommand           { get; }
        public DelegateCommand RemoveSelectedPropertyCommand { get; }

        #endregion

        #region ctor

        public MWVM()
        {
            AddNewFKEntityCommand         = new DelegateCommand(OnAddNewFKEntity);
            SaveFKEntityCommand           = new DelegateCommand(OnSaveFKEntity,         () => FK_Entities.Count(x => x.Id == NewFKEntity.Id) >= 0);
            RemoveSelectedFKEntityCommand = new DelegateCommand(OnRemoveSelectedEntity, () => FK_Entities.IndexOf(SelectedFKEntity)          >= 0);
            AddNewPropertyCommand         = new DelegateCommand(OnAddNewProperty);
            SavePropertyCommand           = new DelegateCommand(OnSaveProperty,           () => Properties.Count(x => x.Id == NewProperty.Id) >= 0);
            RemoveSelectedPropertyCommand = new DelegateCommand(OnRemoveSelectedProperty, () => Properties.IndexOf(SelectedProperty)          >= 0);
            Builder                       = new ConverterBuilder(this);
            NewFKEntity                   = new FKEntity(0);
            NewProperty                   = new Property(0);
            FK_Entities                   = new ObservableCollection<FKEntity>();
            Properties                    = new ObservableCollection<Property>();
        }

        #endregion

        #region private funcs

        private void OnAddNewFKEntity()
        {
            if (NewFKEntity.Id == SelectedFKEntity?.Id)
                NewFKEntity = new FKEntity(FK_Entities.Count);

            if (string.IsNullOrEmpty(NewFKEntity.EntityName))
                return;

            if (string.IsNullOrEmpty(NewFKEntity.NavigationPropertyName))
                return;

            FK_Entities.Add(NewFKEntity);

            NewFKEntity = new FKEntity(FK_Entities.Count);
            Result      = Builder.GetToConverterResult();
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
                Result           = Builder.GetToConverterResult();
            }
        }

        private void OnRemoveSelectedEntity()
        {
            FK_Entities.Remove(SelectedFKEntity);
            SelectedFKEntity = null;

            for (var i = 0; i < FK_Entities.Count; i++)
                FK_Entities[i].Id = i;

            Result = Builder.GetToConverterResult();
        }

        private void RaiseCanExecChanged()
        {
            AddNewFKEntityCommand.RaiseCanExecuteChanged();
            SaveFKEntityCommand.RaiseCanExecuteChanged();
            RemoveSelectedFKEntityCommand.RaiseCanExecuteChanged();
            AddNewPropertyCommand.RaiseCanExecuteChanged();
            SavePropertyCommand.RaiseCanExecuteChanged();
            RemoveSelectedPropertyCommand.RaiseCanExecuteChanged();
        }

        private void OnAddNewProperty()
        {
            if (NewProperty.Id == SelectedProperty?.Id)
                NewProperty = new Property(Properties.Count);

            if (string.IsNullOrEmpty(NewProperty.Name))
                return;

            Properties.Add(NewProperty);

            NewProperty = new Property(Properties.Count);
            Result      = Builder.GetToConverterResult();
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
                Result           = Builder.GetToConverterResult();
            }
        }

        private void OnRemoveSelectedProperty()
        {
            Properties.Remove(SelectedProperty);
            SelectedProperty = null;

            for (var i = 0; i < Properties.Count; i++)
                Properties[i].Id = i;

            Result = Builder.GetToConverterResult();
        }

        #endregion

        #region public funcs



        #endregion
    }
}