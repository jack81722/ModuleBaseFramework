using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased.DAO {
    public interface IGameDaoCollection {
        void AddDao<TItf, TDao>() where TItf : class where TDao : TItf;

        void AddDao<TItf, TDao>(TDao daoInst) where TItf : class where TDao : TItf;

        void AddDao(Type itfType, Type daoType);

        void AddDao(Type itfType, object doaInst);

        TItf GetDao<TItf>() where TItf : class;

        object GetDao(Type itfType);

        IEnumerable GetAllDaos();
    }
}