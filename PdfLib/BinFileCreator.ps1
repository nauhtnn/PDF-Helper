function WriteBinFile {
    param (
        [string]$filePath,
        [string]$text
    )

    # Convert string to bytes (UTF-8 encoding)
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($text)

    # Apply bitwise XOR with a fixed key (0xAA here)
    for ($i = 0; $i -lt $bytes.Length; $i++) {
        $bytes[$i] = $bytes[$i] -bxor 0xAA
    }

    # Write obfuscated bytes to file
    [System.IO.File]::WriteAllBytes($filePath, $bytes)
}

function ReadBinFile {
    param (
        [string]$filePath
    )

    # Read obfuscated bytes from file
    $bytes = [System.IO.File]::ReadAllBytes($filePath)

    # Reverse bitwise XOR with the same key
    for ($i = 0; $i -lt $bytes.Length; $i++) {
        $bytes[$i] = $bytes[$i] -bxor 0xAA
    }

    # Convert back to string
    return [System.Text.Encoding]::UTF8.GetString($bytes)
}

$filePath = "D:\Dev\BadWords.bin"

$PREFIX = "\b(";
$SUFFIX = ")\b";

$badWords = ReadBinFile $filePath

$badWords = $badWords.Substring($PREFIX.Length, $badWords.Length - $PREFIX.Length - $SUFFIX.Length);

$moreBadWords = "";

$badWords = $badWords + $moreBadWords;

WriteBinFile $filePath ($PREFIX + $badWords + $SUFFIX)
