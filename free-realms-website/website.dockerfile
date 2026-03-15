FROM nginx:alpine
COPY createAccountPage.html /usr/share/nginx/html/index.html
EXPOSE 80