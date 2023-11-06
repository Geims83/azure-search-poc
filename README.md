# Azure Cognitive Search end-to-end POC
An E2E Azure CognitiveSearch POC showing the two indexing ways of index managing (push/pull).

## A quick note on Azure Cognitive Search Indexing
There are two ways of populating an index in Azure Cognitive Search:
- Push: in the push mode, an application progammatically send documents, in the JSON format, to the index via the Azure Cognitive Search REST API.
- Pull: in the pull mode, an indexer is created. An indexer is a crawler that extracts searchable content from a data source and populates a search index using field mappings. You can run indexers on demand or on a schedule, and you can also use them to apply AI enrichment to your data.

## Infrastructure needed
- Azure Cognitive Search
- Azure Functions
- Azure Storage account
- Azure Document Intelligence
- Azure SQL Server

## Project structure

### Mgmt
A console application that creates the indexes and the indexer.

### DocDB
A MSSQL server that contains the documents metadata.

### API
An Azure Function application that provides the endpoints for the indexer and the publishing app.
The `Pull` endpoint is a custom WebAPI Skill used by the indexer to enrich the document with data from the database before publishing to the index.
The `Push` endpoint is a queue triggered Durable function that calls the Document Intelligence API to read the content of the document using OCR an than publishes the document to the index.

### APP
A Blazor Web App that can be used to:
- publish document on the indexes
- mark documents as updated or deleted
- search the indexes
