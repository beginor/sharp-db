# 数据库 MCP 工具

一个支持多种数据库的 MCP 服务器，具有如下工具：

- `ExecuteQuery(sql)` 执行 SQL 语句， 以 markdown 表格的形式返回结果；

## 命令行 stdio 模式

在命令行模式下，通过环境变量 `DB_TYPE` 和 `DB_CONN_STR` 连接到数据库，说明如下：

- `DB_TYPE` 为数据库类型，全部小写，支持 postgres、 mysql、 sqlite；
- `DB_CONN_STR` 为标准的 ADO.NET 数据库连接串；

使用示例：

```sh
export DB_TYPE="postgres"
export DB_CONN_STR="server=127.0.0.1;port=5432;database=test_db;user id=postgres;password=pgsql@18"
sharp-db-mcp
```
