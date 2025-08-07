项目结构
WpfProjectToDo：WPF 客户端应用
负责用户界面展示和交互
使用 Prism 框架进行 MVVM 模式开发
集成 MaterialDesignInXAML 主题库，提供现代化 UI
包含多个视图（MainView、MemoView、AboutView、ProgressView 等）
MyToDo.Api：ASP.NET Core Web API 服务
提供后端数据接口
使用 Entity Framework Core 进行数据访问
采用 SQLite 作为数据库
实现了待办事项（ToDo）和备忘录（Memo）的增删改查功能
MyToDo.Shared：共享类库
包含 DTO（数据传输对象）定义
提供分页列表接口和实现
定义 API 响应模型（ApiResponse）
