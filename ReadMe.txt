
Usage:

Index images from the given directory:

    dotnet run -- collect --directory "C:\faces" --collectionID test --access-key YOUR_ACCESS_KEY --secret-key YOUR_SECRET_KEY --region eu-central-1 --pattern "specimen*.jpg"

Search faces by images from the given directory:

    dotnet run -- search --directory "C:\faces" --collectionID test --access-key YOUR_ACCESS_KEY --secret-key YOUR_SECRET_KEY --region eu-central-1 --pattern "input*.jpg"



Help
dotnet run -- --help
