==== NoPrimaryKeyDefined ====

Warns you if there is no primary key defined for a Linq To Sql Table class.

* Checks all classes for declarations that have a System.Data.Linq.Mapping.TableAttribute
* Checks all properties in such classes for declarations that have a System.Data.Linq.Mapping.ColumnAttribute
* Checks that at least one of the properties which has a ColumnAttribute has the argument `IsPrimaryKey == true`

Todo: should also check Fields, but the default behavior is to use Properties.
AFAIK, only LinqPad uses Fields, for speed of generating classes on the fly.

==== WhereCharEquals ====

Warns you if there is a LinqToSql IQueryable expression that uses `char == char`, which leads to poor Sql translations.

* Checks for expressions that are of type `[char|char?] == [char|char?]`
* Traverses up the tree until we can determine if we're in a LinqToSql IQueryable statement or not.
* Checks that the type of IQueryable<T> is something that is defined as a LinqToSql Table (with TableAttribute).

Todo: This may be fairly slow, it is unknown.
We could alternatively look for IQueryable methods first, and then interrogate them for problem expressions.

==== ToUpperLower ====

Warns you if there is a LinqToSql IQueryable expression that uses <string>.ToUpper or <string>.ToLower in anything other than a .Select invocation, as this is unnecessary and can hurt Sql performance.

* Checks for invocations that match ToUpper or ToLower
* Makes sure that those invocations are from the System.String class
* ??? (to be completed)

==== LocalJoinToRemoteData ====

Warns you if there is an IQueryable that is `joined` to local IEnumerable, which will cause the join to be done client-side instead of server-side.

(to be completed)