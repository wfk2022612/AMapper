AMapper
=======
AMapper是一个通过表达式树和MSIL实现的可配置的实体转换器，默认采用忽略大小写的属性名作为映射条件。

使用方法
-----------
1. 引用AMapper.dll
2. 创建类映射关系
```csharp
var _mapFunc = FastMap.CreateMap<AClass, BClass>().Compile();
```
3. 使用生成的委托对类型实例进行转换
```csharp
AClass a = new AClass();
BClass b = _mapFunc(a);
```