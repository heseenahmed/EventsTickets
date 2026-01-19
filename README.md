# Event Management & Ticketing System
![Banner](https://img.shields.io/badge/Status-In--Progress-orange?style=for-the-badge)
![.NET 9](https://img.shields.io/badge/.NET-9.0-blue?style=for-the-badge&logo=dotnet)
![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-green?style=for-the-badge)
![Saudi Arabia](https://img.shields.io/badge/Market-Saudi--Arabia-darkgreen?style=for-the-badge)

A high-performance, scalable solution built with **.NET 9** and **Clean Architecture**, designed to manage large-scale events, graduation parties, and conferences with seamless booking and visitor tracking.

## 🚀 Key Features

*   **Comprehensive Event Lifecycle**: Full CRUD operations for diverse event types including Graduation Parties and Conferences.
*   **Intelligent Booking System**: Dynamic ticket availability management that automatically updates visitor counts and event capacity in real-time.
*   **Multi-Role Authentication**: Secure identity management with ASP.NET Core Identity and JWT integration for Students, Admins, and Visitors.
*   **Rich Media Support**: Integrated image upload and management system for event banners and attendee profiles.
*   **Bilingual Localization**: Full support for English and Arabic languages with professional localized response messages.
*   **Automated Ticketing**: Guest checkout support with photo capture and unique QR code data generation.

## 🛠️ Tech Stack

*   **Backend**: .NET 9 Web API
*   **Architecture**: Clean Architecture (Domain Driven Design)
*   **Database**: SQL Server with Entity Framework Core
*   **Patterns**: CQRS (MediatR), Repository Pattern, Unit of Work
*   **Localization**: Resource-based multi-language support (EN/AR)
*   **Security**: ASP.NET Core Identity & JWT (JSON Web Tokens)

## 🏗️ Technical Architecture

This project follows **Clean Architecture** principles to ensure high maintainability, testability, and separation of concerns:

-   **Domain**: Core entities, enums, and repository interfaces.
-   **Application**: Business logic, CQRS commands/queries, DTOs, and mapping profiles.
-   **Infrastructure**: Data persistence, migrations, and external service integrations (SQL Server).
-   **API**: RESTful endpoints with standardized responses and global error handling.

## 🌍 Localization

The system is designed with a global-first mindset. All API responses are localized based on the `Accept-Language` header, providing clear and professional feedback in both **English** and **Arabic**.
