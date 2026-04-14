using System.Reflection;
using System.Runtime.CompilerServices;
using RealmNexus.Logging;
using RealmNexus.Packets;
using TrProtocol;

namespace RealmNexus.Core;

public class PacketHandlerManager(RealmClient client, ILogger logger = null)
{
    private readonly List<IPacketHandler> _handlers = [];
    private readonly ILogger _logger = logger ?? new ConsoleLogger();
    private readonly RealmClient _client = client;

    private readonly ObjectPool<PacketInterceptArgs> _argsPool = new(() => new PacketInterceptArgs(null!, (INetPacket)null!), 64);

    public void RegisterHandler(IPacketHandler handler)
    {
        _handlers.Add(handler);
        _logger.LogDebug("HandlerManager", $"注册处理器: {handler.GetType().Name}");
    }

    public void RegisterHandlersFromAssembly(Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IPacketHandler).IsAssignableFrom(t))
            .ToList();

        var sortedTypes = TopologicalSort(handlerTypes);

        foreach (var type in sortedTypes)
        {
            try
            {
                var handler = CreateHandler(type);
                if (handler != null)
                {
                    RegisterHandler(handler);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("HandlerManager", $"创建处理器失败 {type.Name}: {ex.Message}");
            }
        }
    }

    public void RegisterHandlersFromCurrentAssembly()
    {
        RegisterHandlersFromAssembly(Assembly.GetExecutingAssembly());
    }

    private List<Type> TopologicalSort(List<Type> types)
    {
        var dependencies = new Dictionary<Type, List<Type>>();
        var inDegree = new Dictionary<Type, int>();

        foreach (var type in types)
        {
            dependencies[type] = [];
            inDegree[type] = 0;
        }

        foreach (var type in types)
        {
            var ctor = type.GetConstructors()
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault();

            if (ctor == null) continue;

            foreach (var param in ctor.GetParameters())
            {
                if (typeof(IPacketHandler).IsAssignableFrom(param.ParameterType))
                {
                    if (types.Contains(param.ParameterType))
                    {
                        dependencies[param.ParameterType].Add(type);
                        inDegree[type]++;
                    }
                }
            }
        }

        var queue = new Queue<Type>();
        var result = new List<Type>();

        foreach (var type in types)
        {
            if (inDegree[type] == 0)
            {
                queue.Enqueue(type);
            }
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);

            foreach (var dependent in dependencies[current])
            {
                inDegree[dependent]--;
                if (inDegree[dependent] == 0)
                {
                    queue.Enqueue(dependent);
                }
            }
        }

        if (result.Count != types.Count)
        {
            var unresolved = types.Except(result).ToList();
            foreach (var type in unresolved)
            {
                _logger.LogError("HandlerManager", $"处理器 {type.Name} 存在循环依赖或无法解决的依赖");
            }
        }

        return result;
    }

    private IPacketHandler? CreateHandler(Type type)
    {
        var ctor = type.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (ctor == null) return null;

        var parameters = ctor.GetParameters();
        var args = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            args[i] = ResolveParameter(param.ParameterType);
        }

        return (IPacketHandler?)ctor.Invoke(args);
    }

    private object? ResolveParameter(Type paramType)
    {
        if (paramType == typeof(RealmClient))
            return _client;

        if (paramType == typeof(ILogger))
            return _logger;

        if (typeof(IPacketHandler).IsAssignableFrom(paramType))
            return _handlers.FirstOrDefault(h => h.GetType() == paramType);

        return null;
    }

    #region 包处理

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ProcessC2S(PacketPipe pipe, INetPacket packet)
    {
        var args = _argsPool.Get();
        args.Reset(pipe, packet);

        try
        {
            var count = _handlers.Count;
            for (var i = 0; i < count; i++)
            {
                var handler = _handlers[i];
                try
                {
                    handler.OnC2S(args);
                    if (args.Handled)
                    {
                        _logger.LogDebug("HandlerManager", $"C2S 包被 {handler.GetType().Name} 拦截: {packet.GetType().Name}");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("HandlerManager", $"C2S 处理器 {handler.GetType().Name} 异常: {ex.Message}");
                }
            }

            return false;
        }
        finally
        {
            _argsPool.Return(args);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ProcessS2C(PacketPipe pipe, INetPacket packet)
    {
        var args = _argsPool.Get();
        args.Reset(pipe, packet);

        try
        {
            var count = _handlers.Count;
            for (var i = 0; i < count; i++)
            {
                var handler = _handlers[i];
                try
                {
                    handler.OnS2C(args);
                    if (args.Handled)
                    {
                        _logger.LogDebug("HandlerManager", $"S2C 包被 {handler.GetType().Name} 拦截: {packet.GetType().Name}");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("HandlerManager", $"S2C 处理器 {handler.GetType().Name} 异常: {ex.Message}");
                }
            }

            return false;
        }
        finally
        {
            _argsPool.Return(args);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ProcessC2S(PacketPipe pipe, ICustomPacket packet)
    {
        var args = _argsPool.Get();
        args.Reset(pipe, packet);

        try
        {
            var count = _handlers.Count;
            for (var i = 0; i < count; i++)
            {
                var handler = _handlers[i];
                try
                {
                    handler.OnC2S(args);
                    if (args.Handled)
                    {
                        _logger.LogDebug("HandlerManager", $"C2S 自定义包被 {handler.GetType().Name} 拦截: {packet.GetType().Name}");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("HandlerManager", $"C2S 处理器 {handler.GetType().Name} 异常: {ex.Message}");
                }
            }

            return false;
        }
        finally
        {
            _argsPool.Return(args);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ProcessS2C(PacketPipe pipe, ICustomPacket packet)
    {
        var args = _argsPool.Get();
        args.Reset(pipe, packet);

        try
        {
            var count = _handlers.Count;
            for (var i = 0; i < count; i++)
            {
                var handler = _handlers[i];
                try
                {
                    handler.OnS2C(args);
                    if (args.Handled)
                    {
                        _logger.LogDebug("HandlerManager", $"S2C 自定义包被 {handler.GetType().Name} 拦截: {packet.GetType().Name}");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("HandlerManager", $"S2C 处理器 {handler.GetType().Name} 异常: {ex.Message}");
                }
            }

            return false;
        }
        finally
        {
            _argsPool.Return(args);
        }
    }

    #endregion

    #region 生命周期事件

    public void OnConnected()
    {
        foreach (var handler in _handlers)
        {
            try
            {
                handler.OnConnected();
            }
            catch (Exception ex)
            {
                _logger.LogError("HandlerManager", $"OnConnected {handler.GetType().Name} 异常: {ex.Message}");
            }
        }
    }

    public void OnDisconnected()
    {
        foreach (var handler in _handlers)
        {
            try
            {
                handler.OnDisconnected();
            }
            catch (Exception ex)
            {
                _logger.LogError("HandlerManager", $"OnDisconnected {handler.GetType().Name} 异常: {ex.Message}");
            }
        }
    }

    public void OnServerChanging()
    {
        foreach (var handler in _handlers)
        {
            try
            {
                handler.OnServerChanging();
            }
            catch (Exception ex)
            {
                _logger.LogError("HandlerManager", $"OnServerChanging {handler.GetType().Name} 异常: {ex.Message}");
            }
        }
    }

    public void OnServerChanged()
    {
        foreach (var handler in _handlers)
        {
            try
            {
                handler.OnServerChanged();
            }
            catch (Exception ex)
            {
                _logger.LogError("HandlerManager", $"OnServerChanged {handler.GetType().Name} 异常: {ex.Message}");
            }
        }
    }


    #endregion

    public T? GetHandler<T>() where T : class, IPacketHandler
    {
        return _handlers.FirstOrDefault(h => h is T) as T;
    }
}

public class ObjectPool<T>(Func<T> factory, int capacity) where T : class
{
    private readonly Func<T> _factory = factory;
    private readonly T[] _items = new T[capacity];
    private int _index;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get()
    {
        var index = Interlocked.Decrement(ref _index);
        if (index >= 0 && index < _items.Length)
        {
            var item = _items[index];
            if (item != null)
            {
                _items[index] = null!;
                return item;
            }
        }
        Interlocked.Increment(ref _index);
        return _factory();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return(T item)
    {
        var index = Interlocked.Increment(ref _index) - 1;
        if (index >= 0 && index < _items.Length)
        {
            _items[index] = item;
        }
        else
        {
            Interlocked.Decrement(ref _index);
        }
    }
}
