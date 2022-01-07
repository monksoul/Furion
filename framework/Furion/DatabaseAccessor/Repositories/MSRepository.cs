// Copyright (c) 2020-2022 百小僧, Baiqian Co.,Ltd.
// Furion is licensed under Mulan PSL v2.
// You can use this software according to the terms and conditions of the Mulan PSL v2. 
// You may obtain a copy of Mulan PSL v2 at:
//             https://gitee.com/dotnetchina/Furion/blob/master/LICENSE 
// THIS SOFTWARE IS PROVIDED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO NON-INFRINGEMENT, MERCHANTABILITY OR FIT FOR A PARTICULAR PURPOSE.  
// See the Mulan PSL v2 for more details.

using Furion.DependencyInjection;
using System;
using System.Security.Cryptography;

namespace Furion.DatabaseAccessor
{
    /// <summary>
    /// 默认主库主从仓储
    /// </summary>
    [SuppressSniffer]
    public partial class MSRepository : MSRepository<MasterDbContextLocator>, IMSRepository
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="repository">非泛型仓储</param>
        public MSRepository(IServiceProvider serviceProvider
            , IRepository repository) : base(serviceProvider, repository)
        {
        }
    }

    /// <summary>
    /// 主从库仓储
    /// </summary>
    /// <typeparam name="TMasterDbContextLocator">主库</typeparam>
    [SuppressSniffer]
    public partial class MSRepository<TMasterDbContextLocator> : IMSRepository<TMasterDbContextLocator>
        where TMasterDbContextLocator : class, IDbContextLocator
    {
        /// <summary>
        /// 服务提供器
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 非泛型仓储
        /// </summary>
        private readonly IRepository _repository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="repository">非泛型仓储</param>
        public MSRepository(IServiceProvider serviceProvider
            , IRepository repository)
        {
            _serviceProvider = serviceProvider;
            _repository = repository;
        }

        /// <summary>
        /// 获取主库仓储
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        public virtual IRepository<TEntity, TMasterDbContextLocator> Master<TEntity>()
            where TEntity : class, IPrivateEntity, new()
        {
            return _repository.Change<TEntity, TMasterDbContextLocator>();
        }

        /// <summary>
        /// 动态获取从库（随机）
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        public virtual IPrivateReadableRepository<TEntity> Slave<TEntity>()
            where TEntity : class, IPrivateEntity, new()
        {
            // 判断数据库主库是否注册
            Penetrates.CheckDbContextLocator(typeof(TMasterDbContextLocator), out var dbContextType);

            // 获取主库贴的特性
            var appDbContextAttribute = DbProvider.GetAppDbContextAttribute(dbContextType);

            // 获取从库列表
            var slaveDbContextLocators = appDbContextAttribute.SlaveDbContextLocators;

            // 如果没有定义从库定位器，则抛出异常
            if (slaveDbContextLocators == null || slaveDbContextLocators.Length == 0) throw new InvalidOperationException("Not found slave locators.");

            // 如果只配置了一个从库，直接返回
            if (slaveDbContextLocators.Length == 1) return Slave<TEntity>(() => slaveDbContextLocators[0]);

            // 获取随机从库索引
            var index = RandomNumberGenerator.GetInt32(0, slaveDbContextLocators.Length);

            // 返回随机从库
            return Slave<TEntity>(() => slaveDbContextLocators[index]);
        }

        /// <summary>
        /// 动态获取从库（自定义）
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        public virtual IPrivateReadableRepository<TEntity> Slave<TEntity>(Func<Type> locatorHandler)
            where TEntity : class, IPrivateEntity, new()
        {
            if (locatorHandler == null) throw new ArgumentNullException(nameof(locatorHandler));

            // 获取定位器类型
            var dbContextLocatorType = locatorHandler();
            if (!typeof(IDbContextLocator).IsAssignableFrom(dbContextLocatorType)) throw new InvalidCastException($"{dbContextLocatorType.Name} is not assignable from {nameof(IDbContextLocator)}.");

            // 判断从库定位器是否绑定
            Penetrates.CheckDbContextLocator(dbContextLocatorType, out _);

            // 解析从库定位器
            var repository = _serviceProvider.GetService(typeof(IRepository<,>).MakeGenericType(typeof(TEntity), dbContextLocatorType)) as IPrivateRepository<TEntity>;

            // 返回从库仓储
            return repository.Constraint<IPrivateReadableRepository<TEntity>>();
        }
    }

    /// <summary>
    /// 主从库仓储
    /// </summary>
    /// <typeparam name="TMasterDbContextLocator">主库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator1">从库</typeparam>
    [SuppressSniffer]
    public partial class MSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1> : IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1>
        where TMasterDbContextLocator : class, IDbContextLocator
        where TSlaveDbContextLocator1 : class, IDbContextLocator
    {
        /// <summary>
        /// 非泛型仓储
        /// </summary>
        private readonly IRepository _repository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository">非泛型仓储</param>
        public MSRepository(IRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 获取主库仓储
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        public virtual IRepository<TEntity, TMasterDbContextLocator> Master<TEntity>()
            where TEntity : class, IPrivateEntity, new()
        {
            return _repository.Change<TEntity, TMasterDbContextLocator>();
        }

        /// <summary>
        /// 获取从库仓储
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        public virtual IReadableRepository<TEntity, TSlaveDbContextLocator1> Slave1<TEntity>()
            where TEntity : class, IPrivateEntity, new()
        {
            return _repository.Change<TEntity, TSlaveDbContextLocator1>().Constraint<IReadableRepository<TEntity, TSlaveDbContextLocator1>>();
        }
    }

    /// <summary>
    /// 主从库仓储
    /// </summary>
    /// <typeparam name="TMasterDbContextLocator">主库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator1">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator2">从库</typeparam>
    [SuppressSniffer]
    public partial class MSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2>
        : MSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1>
        , IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2>
        where TMasterDbContextLocator : class, IDbContextLocator
        where TSlaveDbContextLocator1 : class, IDbContextLocator
        where TSlaveDbContextLocator2 : class, IDbContextLocator
    {
        /// <summary>
        /// 非泛型仓储
        /// </summary>
        private readonly IRepository _repository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository">非泛型仓储</param>
        public MSRepository(IRepository repository) : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 获取从库仓储2
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        public virtual IReadableRepository<TEntity, TSlaveDbContextLocator2> Slave2<TEntity>()
            where TEntity : class, IPrivateEntity, new()
        {
            return _repository.Change<TEntity, TSlaveDbContextLocator2>().Constraint<IReadableRepository<TEntity, TSlaveDbContextLocator2>>();
        }
    }

    /// <summary>
    /// 主从库仓储
    /// </summary>
    /// <typeparam name="TMasterDbContextLocator">主库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator1">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator2">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator3">从库</typeparam>
    [SuppressSniffer]
    public partial class MSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3>
        : MSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2>
        , IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3>
        where TMasterDbContextLocator : class, IDbContextLocator
        where TSlaveDbContextLocator1 : class, IDbContextLocator
        where TSlaveDbContextLocator2 : class, IDbContextLocator
        where TSlaveDbContextLocator3 : class, IDbContextLocator
    {
        /// <summary>
        /// 非泛型仓储
        /// </summary>
        private readonly IRepository _repository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository">非泛型仓储</param>
        public MSRepository(IRepository repository) : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 获取从库仓储3
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        public virtual IReadableRepository<TEntity, TSlaveDbContextLocator3> Slave3<TEntity>()
            where TEntity : class, IPrivateEntity, new()
        {
            return _repository.Change<TEntity, TSlaveDbContextLocator3>().Constraint<IReadableRepository<TEntity, TSlaveDbContextLocator3>>();
        }
    }

    /// <summary>
    /// 主从库仓储
    /// </summary>
    /// <typeparam name="TMasterDbContextLocator">主库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator1">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator2">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator3">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator4">从库</typeparam>
    [SuppressSniffer]
    public partial class MSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4>
        : MSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3>
        , IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4>
        where TMasterDbContextLocator : class, IDbContextLocator
        where TSlaveDbContextLocator1 : class, IDbContextLocator
        where TSlaveDbContextLocator2 : class, IDbContextLocator
        where TSlaveDbContextLocator3 : class, IDbContextLocator
        where TSlaveDbContextLocator4 : class, IDbContextLocator
    {
        /// <summary>
        /// 非泛型仓储
        /// </summary>
        private readonly IRepository _repository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository">非泛型仓储</param>
        public MSRepository(IRepository repository) : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 获取从库仓储4
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        public virtual IReadableRepository<TEntity, TSlaveDbContextLocator4> Slave4<TEntity>()
            where TEntity : class, IPrivateEntity, new()
        {
            return _repository.Change<TEntity, TSlaveDbContextLocator4>().Constraint<IReadableRepository<TEntity, TSlaveDbContextLocator4>>();
        }
    }

    /// <summary>
    /// 主从库仓储
    /// </summary>
    /// <typeparam name="TMasterDbContextLocator">主库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator1">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator2">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator3">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator4">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator5">从库</typeparam>
    [SuppressSniffer]
    public partial class MSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4, TSlaveDbContextLocator5>
        : MSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4>
        , IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4, TSlaveDbContextLocator5>
        where TMasterDbContextLocator : class, IDbContextLocator
        where TSlaveDbContextLocator1 : class, IDbContextLocator
        where TSlaveDbContextLocator2 : class, IDbContextLocator
        where TSlaveDbContextLocator3 : class, IDbContextLocator
        where TSlaveDbContextLocator4 : class, IDbContextLocator
        where TSlaveDbContextLocator5 : class, IDbContextLocator
    {
        /// <summary>
        /// 非泛型仓储
        /// </summary>
        private readonly IRepository _repository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository">非泛型仓储</param>
        public MSRepository(IRepository repository) : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 获取从库仓储5
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        public virtual IReadableRepository<TEntity, TSlaveDbContextLocator5> Slave5<TEntity>()
            where TEntity : class, IPrivateEntity, new()
        {
            return _repository.Change<TEntity, TSlaveDbContextLocator5>().Constraint<IReadableRepository<TEntity, TSlaveDbContextLocator5>>();
        }
    }

    /// <summary>
    /// 主从库仓储
    /// </summary>
    /// <typeparam name="TMasterDbContextLocator">主库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator1">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator2">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator3">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator4">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator5">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator6">从库</typeparam>
    [SuppressSniffer]
    public partial class MSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4, TSlaveDbContextLocator5, TSlaveDbContextLocator6>
        : MSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4, TSlaveDbContextLocator5>
        , IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4, TSlaveDbContextLocator5, TSlaveDbContextLocator6>
        where TMasterDbContextLocator : class, IDbContextLocator
        where TSlaveDbContextLocator1 : class, IDbContextLocator
        where TSlaveDbContextLocator2 : class, IDbContextLocator
        where TSlaveDbContextLocator3 : class, IDbContextLocator
        where TSlaveDbContextLocator4 : class, IDbContextLocator
        where TSlaveDbContextLocator5 : class, IDbContextLocator
        where TSlaveDbContextLocator6 : class, IDbContextLocator
    {
        /// <summary>
        /// 非泛型仓储
        /// </summary>
        private readonly IRepository _repository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository">非泛型仓储</param>
        public MSRepository(IRepository repository) : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 获取从库仓储6
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        public virtual IReadableRepository<TEntity, TSlaveDbContextLocator6> Slave6<TEntity>()
            where TEntity : class, IPrivateEntity, new()
        {
            return _repository.Change<TEntity, TSlaveDbContextLocator6>().Constraint<IReadableRepository<TEntity, TSlaveDbContextLocator6>>();
        }
    }

    /// <summary>
    /// 主从库仓储
    /// </summary>
    /// <typeparam name="TMasterDbContextLocator">主库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator1">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator2">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator3">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator4">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator5">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator6">从库</typeparam>
    /// <typeparam name="TSlaveDbContextLocator7">从库</typeparam>
    [SuppressSniffer]
    public partial class MSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4, TSlaveDbContextLocator5, TSlaveDbContextLocator6, TSlaveDbContextLocator7>
        : MSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4, TSlaveDbContextLocator5, TSlaveDbContextLocator6>
        , IMSRepository<TMasterDbContextLocator, TSlaveDbContextLocator1, TSlaveDbContextLocator2, TSlaveDbContextLocator3, TSlaveDbContextLocator4, TSlaveDbContextLocator5, TSlaveDbContextLocator6, TSlaveDbContextLocator7>
        where TMasterDbContextLocator : class, IDbContextLocator
        where TSlaveDbContextLocator1 : class, IDbContextLocator
        where TSlaveDbContextLocator2 : class, IDbContextLocator
        where TSlaveDbContextLocator3 : class, IDbContextLocator
        where TSlaveDbContextLocator4 : class, IDbContextLocator
        where TSlaveDbContextLocator5 : class, IDbContextLocator
        where TSlaveDbContextLocator6 : class, IDbContextLocator
        where TSlaveDbContextLocator7 : class, IDbContextLocator
    {
        /// <summary>
        /// 非泛型仓储
        /// </summary>
        private readonly IRepository _repository;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="repository">非泛型仓储</param>
        public MSRepository(IRepository repository) : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 获取从库仓储7
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns></returns>
        public virtual IReadableRepository<TEntity, TSlaveDbContextLocator7> Slave7<TEntity>()
            where TEntity : class, IPrivateEntity, new()
        {
            return _repository.Change<TEntity, TSlaveDbContextLocator7>().Constraint<IReadableRepository<TEntity, TSlaveDbContextLocator7>>();
        }
    }
}