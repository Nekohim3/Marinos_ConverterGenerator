using System;
using Microsoft.Practices.Prism.ViewModel;

namespace Marinos_ConverterGenerator
{
    [Serializable]
    public class FKEntity : NotificationObject
    {
        private int    _id;
        private string _entityName;
        private string _navigationPropertyName;
        private bool   _nullable;

        public int Id
        {
            get => _id;
            set { _id = value; RaisePropertyChanged(() => Id);}
        }

        public string EntityName
        {
            get => _entityName;
            set { _entityName = value; RaisePropertyChanged(() => EntityName);}
        }

        public string NavigationPropertyName
        {
            get => _navigationPropertyName;
            set { _navigationPropertyName = value;RaisePropertyChanged(() => NavigationPropertyName); }
        }

        public bool Nullable
        {
            get => _nullable;
            set { _nullable = value;RaisePropertyChanged(() => Nullable); }
        }

        public FKEntity()
        {
        }

        public FKEntity(int id)
        {
            Id = id;
        }

        public FKEntity(FKEntity old)
        {
            if(old == null) return;
            Id                     = old.Id;
            EntityName             = old.EntityName;
            NavigationPropertyName = old.NavigationPropertyName;
            Nullable               = old.Nullable;
        }
        
        public override string ToString()
        {
            return $"{EntityName}->{NavigationPropertyName}{(Nullable ? " NULL" : "")}";
        }
    }
}