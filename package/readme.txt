FasterMapper是一个通过表达式树和MSIL实现的可配置的实体转换器。

FastMapper默认采用忽略大小写的属性名作为映射条件。

使用方法：
1.引用FastMapper.dll
2.创建类映射关系
var _mapFunc = FastMap.CreateMap<AClass, BClass>().Compile();
3.使用生成的委托对类型实例进行转换
AClass a = new AClass();
BClass b = _mapFunc(a);

v1.0 beta 2018-06-01
1.支持集合属性
2.支持基本类型间自动转换
3.修改BUG，未配置的属性值为null时，转换出错
