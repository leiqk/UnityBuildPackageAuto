
echo "开始打包"

version="1.1.1"
channel="1"
isNeedAB="1"
versionMode="1"
typePlatform="1"

read -p "请输入版本号(1.1.2)：" version
read -p "请输入渠道号(1=ymcx, 2=zjtd)：" channel
read -p "是否需要打ab（0 否， 1 是）：" isNeedAB
read -p "版本模式（0 dev 1 test 2 preview 3 release）：" versionMode

echo "版本：version = $version"
echo "渠道：channel = $channel"
echo "是否AB：isNeedAB = $isNeedAB"
echo "版本模式：versionMode = $versionMode"
echo "平台类型：typePlatform = $typePlatform"

projectPath="Mgx"

"$UNITY_PATH_2019" -quit -batchmode -projectPath $projectPath -executeMethod ProjectBuild.BuildProjected version="$version" InstallVersion="$InstallVersion" channel="$channel" isNeedAB="$isNeedAB" versionMode="$versionMode" typePlatform="$typePlatform"

