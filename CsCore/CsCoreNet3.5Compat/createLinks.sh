#!/bin/bash
echo "Format: createLinks.sh <destination>"

src=$PWD

if [[ $# < 1 ]]; then
	echo "Destination directory - Unity Project"
    echo "Copy+Paste the project folder path the component should be placed in:"
	read tmp_dest
else
	tmp_dest="$@"
fi

# get abs path of destination
if [[ $tmp_dest == /* ]]; then
    echo "Destination: found absolute path: "$tmp_dest
	dest="$tmp_dest"
else
    echo "Destination: found relative path: "$tmp_dest
	dest="$PWD/$tmp_dest"
fi

echo "SRC: " $src
echo "DEST: " $dest

if [ ! -d "$dest" ]; then
  # Control will enter here if $DIRECTORY doesn't exist.
  echo "Destination directory does not exist: "$dest
  exit
fi

cd "$dest"
echo cd $PWD

echo "Searching for Assets directory.."

if [ ! -d "Assets" ]; then
    echo "Error: Assets directory not found."
    echo "Destination: Unity Project, should contain an Assets folder"
    exit 1
fi

echo "SUCCESS: Assets folder found."

dest="$dest/Assets"

cd "$src"
echo cd $PWD

echo "Create symlinks.."


for dir in */; do
	dir=${dir%/}
	echo "create directory: $dest/$dir"
	mkdir -p "$dest/$dir"
	cd "$dir"
	echo cd $PWD

	for base in */; do
		base=${base%/}
		if [ "$(ls -d $base)" ]; then # Folder not empty
			if [[ ($base == "Android" || $base == "iOS" || $base == "OSX") ]]; then
				mkdir -p "$dest/$dir/$base"
				cd "$base"
				echo cd $PWD
				for pluginBase in */; do
					pluginBase=${pluginBase%/}
					if [ "$(ls -d $pluginBase)" ]; then # Folder not empty
						echo "Link (Plugin) directory: $src/$dir/$base/$pluginBase to $dest/$dir/$base/$pluginBase"
						ln -s "$src/$dir/$base/$pluginBase" "$dest/$dir/$base/$pluginBase"
					fi
				done
				cd ..
				echo cd $PWD
			else
				echo "Link directory: $src/$dir/$base to $dest/$dir/$base"
				ln -s "$src/$dir/$base" "$dest/$dir/$base"
				# echo "$target"
			fi
		fi
	done

	cd ..

done