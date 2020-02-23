cd ~/JW_Management

git clone https://github.com/JKSProds/JW_Management.git

cd JW_Management/JW_Management

docker build -t jwmanagement .

docker stop JW_Management
docker rm JW_Management

cd ..
cd ..

chmod -R 777 JW_Management

rm -r JW_Management

docker run -d -p 8081:80 --restart always --name JW_Management jwmanagement



