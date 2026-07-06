namespace Beginor.SharpDbMcp;

internal sealed class PostgresMetadataProvider : BaseMetadataProvider {

    public PostgresMetadataProvider(IDbConnectionFactory connectionFactory, DatabaseOptions options)
        : base(connectionFactory, options) {
    }

    protected override string GetTablesQuery() {
        return """
            with primary_keys as (
                select key_usage.table_schema,
                       key_usage.table_name,
                       string_agg(key_usage.column_name, ', ' order by key_usage.ordinal_position) as primary_key_columns
                from information_schema.table_constraints constraints
                join information_schema.key_column_usage key_usage
                  on key_usage.constraint_schema = constraints.constraint_schema
                 and key_usage.constraint_name = constraints.constraint_name
                 and key_usage.table_schema = constraints.table_schema
                 and key_usage.table_name = constraints.table_name
                where constraints.constraint_type = 'PRIMARY KEY'
                group by key_usage.table_schema, key_usage.table_name
            ),
            foreign_keys as (
                select key_usage.table_schema,
                       key_usage.table_name,
                       string_agg(
                           key_usage.column_name || ' -> ' ||
                           column_usage.table_schema || '.' ||
                           column_usage.table_name || '(' ||
                           column_usage.column_name || ')',
                           '; '
                           order by key_usage.ordinal_position
                       ) as foreign_keys,
                       string_agg(
                           distinct column_usage.table_schema || '.' || column_usage.table_name,
                           ', '
                       ) as related_objects
                from information_schema.table_constraints constraints
                join information_schema.key_column_usage key_usage
                  on key_usage.constraint_schema = constraints.constraint_schema
                 and key_usage.constraint_name = constraints.constraint_name
                 and key_usage.table_schema = constraints.table_schema
                 and key_usage.table_name = constraints.table_name
                join information_schema.constraint_column_usage column_usage
                  on column_usage.constraint_schema = constraints.constraint_schema
                 and column_usage.constraint_name = constraints.constraint_name
                where constraints.constraint_type = 'FOREIGN KEY'
                group by key_usage.table_schema, key_usage.table_name
            )
            select tables.table_schema,
                   tables.table_name,
                   tables.table_type,
                   obj_description(classes.oid, 'pg_class') as table_description,
                   primary_keys.primary_key_columns,
                   foreign_keys.foreign_keys,
                   foreign_keys.related_objects
            from information_schema.tables tables
            join pg_namespace namespaces
              on namespaces.nspname = tables.table_schema
            join pg_class classes
              on classes.relnamespace = namespaces.oid
             and classes.relname = tables.table_name
            left join primary_keys
              on primary_keys.table_schema = tables.table_schema
             and primary_keys.table_name = tables.table_name
            left join foreign_keys
              on foreign_keys.table_schema = tables.table_schema
             and foreign_keys.table_name = tables.table_name
            where tables.table_schema not in ('pg_catalog', 'information_schema')
              and tables.table_type in ('BASE TABLE', 'VIEW')
              and (@schema is null or @schema = '' or tables.table_schema = @schema)
            order by tables.table_schema, tables.table_name
            """;
    }

    protected override string GetColumnsQuery() {
        return """
            with primary_key_columns as (
                select key_usage.table_schema,
                       key_usage.table_name,
                       key_usage.column_name
                from information_schema.table_constraints constraints
                join information_schema.key_column_usage key_usage
                  on key_usage.constraint_schema = constraints.constraint_schema
                 and key_usage.constraint_name = constraints.constraint_name
                 and key_usage.table_schema = constraints.table_schema
                 and key_usage.table_name = constraints.table_name
                where constraints.constraint_type = 'PRIMARY KEY'
            ),
            foreign_key_columns as (
                select key_usage.table_schema,
                       key_usage.table_name,
                       key_usage.column_name,
                       column_usage.table_schema as referenced_table_schema,
                       column_usage.table_name as referenced_table_name,
                       column_usage.column_name as referenced_column_name
                from information_schema.table_constraints constraints
                join information_schema.key_column_usage key_usage
                  on key_usage.constraint_schema = constraints.constraint_schema
                 and key_usage.constraint_name = constraints.constraint_name
                 and key_usage.table_schema = constraints.table_schema
                 and key_usage.table_name = constraints.table_name
                join information_schema.constraint_column_usage column_usage
                  on column_usage.constraint_schema = constraints.constraint_schema
                 and column_usage.constraint_name = constraints.constraint_name
                where constraints.constraint_type = 'FOREIGN KEY'
            )
            select columns.table_schema,
                   columns.table_name,
                   columns.column_name,
                   columns.ordinal_position,
                   columns.data_type,
                   columns.is_nullable,
                   columns.column_default,
                   col_description(classes.oid, attributes.attnum) as column_description,
                   case when primary_key_columns.column_name is null then 'NO' else 'YES' end as is_primary_key,
                   case when foreign_key_columns.column_name is null then 'NO' else 'YES' end as is_foreign_key,
                   foreign_key_columns.referenced_table_schema,
                   foreign_key_columns.referenced_table_name,
                   foreign_key_columns.referenced_column_name
            from information_schema.columns columns
            join pg_namespace namespaces
              on namespaces.nspname = columns.table_schema
            join pg_class classes
              on classes.relnamespace = namespaces.oid
             and classes.relname = columns.table_name
            join pg_attribute attributes
              on attributes.attrelid = classes.oid
             and attributes.attname = columns.column_name
            left join primary_key_columns
              on primary_key_columns.table_schema = columns.table_schema
             and primary_key_columns.table_name = columns.table_name
             and primary_key_columns.column_name = columns.column_name
            left join foreign_key_columns
              on foreign_key_columns.table_schema = columns.table_schema
             and foreign_key_columns.table_name = columns.table_name
             and foreign_key_columns.column_name = columns.column_name
            where columns.table_schema not in ('pg_catalog', 'information_schema')
              and columns.table_name = @tableName
              and (@schema is null or @schema = '' or columns.table_schema = @schema)
            order by columns.table_schema, columns.table_name, columns.ordinal_position
            """;
    }

}