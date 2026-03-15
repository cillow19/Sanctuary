FROM node:20-alpine
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
EXPOSE 8000
ENV DB_FILE=/data/sanctuary.db
CMD ["node", "server/server.js"]