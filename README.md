# Slip

Slip is a software project created during Vision Critical annual hackathon, and eventually placed third overall. The purpose of this project is to create a middle-man program that facilates communication across different applications, between Slack users and Ring Central users. Slip receives messages incoming from both Slack and Ring Central, temporarily stores them in AWS SQS, before retrieving them in the correct order and sends out the corresponding applications.
This repository represents only the middleware that communicates with Slack bot, Glip bot and AWS services. Slack bot and Glip bot are in separate private repositories.

## Specifications

The project is developed in .NET Core framework. Slip uses AWS SQS as a temporary database, and AWS Elastic Beanstalk for publishing.
