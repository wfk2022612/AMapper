AMapper
=======
AMapper是一个高性能实体转换组件，默认采用忽略大小写的属性名作为映射条件，支持基本类型间的自动转换。对于复杂的集合属性也能很好的支持。在千万级转换测试中，其效率已超过绝大多数Mapper组件。

使用方法
-----------
1. 引用`AMapper.dll`或者使用`nuget`安装
```
Install-Package AMapper
```
2. 创建类映射关系
```csharp
var _mapFunc = Map.Create<AClass, BClass>().Compile();
```
3. 使用生成的委托对类型实例进行转换
```csharp
AClass a = new AClass();
BClass b = _mapFunc(a);
```

4. 集合类型支持一维数组、基本泛型集合类型,自定义集合需要实现IEnumerable<>接口
如 
```csharp
int[] string[] List<> IEnumerable<>
```


