# NachtWiesel.Web.Files.Minio

![NuGet Version](https://img.shields.io/nuget/v/NachtWiesel.Web.Files.Minio)

Небольшая библиотека для упрошённой работы с файлами через S3 Minio-like при разработке веб-приложений и не только.

Представлено 3 инструмента:
- MinioFileReaderService - сервис чтения файлов.

	**Фичи**: чтение и сразу стриминг в HttpResponse.
- MinioFileWriterService - сервис создания файлов.

	**Фичи**: автоматическое присвоение Guid (с проверкой на существование) при создании файла, запись из IFormFile, запись из IBrowserFile
- MinioArchiverService - сервис создания архивов. 

	**Фичи**: создание и стриминг архива без обращения к диску (на запись) и оптимизированным потреблением оперативной памяти.

Для использования инструментов необходима возможность получения сервиса через DependencyInjection.

## Настройка

Для настройки необходимо создать конфигурацию и присвоить ей имя (именно оно передётся первым параметром).

В настройках всех сервисов есть несколько паметров:

Эндпоинт вашего S3 Minio-like сервиса

    string Endpoint

Access-ключ

    string AccessKey

Secret-ключ

    string SecretKey

Начало пути которое будет автоматически приписываться перед filePath при любой операцией (этот параметр не обязателен)

    string BasePath

Имя бакета с которым будет вестись работа

    string BucketName

Далее пример настройки:

    builder.Services.AddMinioFileServices("STORAGE", x =>
    {
        x.Endpoint = "minio.nachtwiesel.ru";
        x.AccessKey = "access";
        x.SecretKey = "secret";
        x.BasePath = "/BasicFiles";
        x.BucketName = "testing-bucket";
    });

## Использование

1. Внедрите зависимость IMinioFileWriterFactory/IMinioFileReaderFactory/IMinioArchiverFactory в ваш сервис
2. Вызовите метод Create("Сюда вставьте название конфигурации")
3. Пользуйтесь