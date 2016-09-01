#FastMapper
FasterMapper是一个使用MSIL（微软中间语言）实现的简单的实体转换器，在千万级实体转换上效率与原生静态代码转换相差10%左右，足以适应大多数转换需求。
但缺点是功能太少，只能对Class类型转换，且仅支持核心的转换功能及简单的属性映射功能，由于用到了表达式树，所以最低支持.net3.5

使用方法：
1.引用FastMapper.dll
2.创建类映射关系
var _mapFunc = FastMap.CreateMap<AClass, BClass>().Compile();
3.使用生成的委托对类型实例进行转换
AClass a = new AClass();
BClass b = _mapFunc(a);
