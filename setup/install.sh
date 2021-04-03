#!/usr/bin/env bash

# Stop script on NZEC
set -e
# Stop script if unbound variable found (use ${var:-} if intentional)
set -u
# By default cmd1 | cmd2 returns exit code of cmd2 regardless of cmd1 success
# This is causing it to fail
set -o pipefail

#Script parameters

#repo owner
owner='Gsonovb'

#repo name
repo='DockerVolumeFileWatcher'

#release tag
version='Latest'

#app Name
appname='filewatcher'

#Debug for this
verbose=false

#Report issue Url
issueurl="https://github.com/$owner/$repo/issues"

#Get Lastest releases URL
latesturl="https://api.github.com/repos/$owner/$repo/releases/latest"

architecture=''
osname=''
distro_osname=''
distro_osversion=''

temporary_file_template="${TMPDIR:-/tmp}/$repo.XXXXXXXXX"
install_root='/usr/bin'
override_files=true

invocation='say_verbose "Calling: ${yellow:-}${FUNCNAME[0]} ${green:-}$*${normal:-}"'

exec 3>&1

if [ -t 1 ] && command -v tput >/dev/null; then
    # see if it supports colors
    ncolors=$(tput colors)
    if [ -n "$ncolors" ] && [ $ncolors -ge 8 ]; then
        bold="$(tput bold || echo)"
        normal="$(tput sgr0 || echo)"
        black="$(tput setaf 0 || echo)"
        red="$(tput setaf 1 || echo)"
        green="$(tput setaf 2 || echo)"
        yellow="$(tput setaf 3 || echo)"
        blue="$(tput setaf 4 || echo)"
        magenta="$(tput setaf 5 || echo)"
        cyan="$(tput setaf 6 || echo)"
        white="$(tput setaf 7 || echo)"
    fi
fi

say_warning() {
    printf "%b\n" "${yellow:-}install: Warning: $1${normal:-}" >&3
}

say_err() {
    printf "%b\n" "${red:-}install: Error: $1${normal:-}" >&2
}

say() {
    printf "%b\n" "${cyan:-}install:${normal:-} $1" >&3
}

say_verbose() {
    if [ "$verbose" = true ]; then
        say "$1"
    fi
}

get_machine_architecture() {
    eval $invocation

    if command -v uname >/dev/null; then
        CPUName=$(uname -m)
        case $CPUName in
        armv*l)
            echo "arm"
            return 0
            ;;
        aarch64 | arm64)
            echo "arm64"
            return 0
            ;;
        i686)
            echo "x86"
            return 0
            ;;
        esac
    fi

    # Always default to 'x64'
    echo "x64"
    return 0
}

get_linux_platform_name() {
    eval $invocation

    if [ -e /etc/os-release ]; then
        . /etc/os-release
        echo "$ID${VERSION_ID:+.${VERSION_ID}}"
        return 0
    elif [ -e /etc/redhat-release ]; then
        local redhatRelease=$(</etc/redhat-release)
        if [[ $redhatRelease == "CentOS release 6."* || $redhatRelease == "Red Hat Enterprise Linux "*" release 6."* ]]; then
            echo "rhel.6"
            return 0
        fi
    fi

    say_verbose "Linux specific platform name and version could not be detected: UName = $uname"
    return 1
}

is_musl_based_distro() {
    (ldd --version 2>&1 || true) | grep -q musl
}

get_current_os_name() {
    eval $invocation

    local uname=$(uname)
    if [ "$uname" = "Darwin" ]; then
        echo "osx"
        return 0
    elif [ "$uname" = "FreeBSD" ]; then
        echo "freebsd"
        return 0
    elif [ "$uname" = "Linux" ]; then
        local linux_platform_name
        linux_platform_name="$(get_linux_platform_name)" || { echo "linux" && return 0; }

        if [ "$linux_platform_name" = "rhel.6" ]; then
            echo $linux_platform_name
            return 0
        elif is_musl_based_distro; then
            echo "linux-musl"
            return 0
        elif [ "$linux_platform_name" = "linux-musl" ]; then
            echo "linux-musl"
            return 0
        else
            echo "linux"
            return 0
        fi
    fi

    say_err "OS name could not be detected: UName = $uname"
    return 1
}

get_distro_os_name() {
    eval $invocation

    local uname=$(uname)
    if [ "$uname" = "Darwin" ]; then
        echo "osx"
        return 0
    else
        if [ -e /etc/os-release ]; then
            . /etc/os-release
            local os=$ID
            if [ -n "$os" ]; then

                echo "$os"
                return 0
            fi
        elif [ -e /etc/redhat-release ]; then
            local redhatRelease=$(</etc/redhat-release)
            if [[ $redhatRelease == "CentOS release 6."* || $redhatRelease == "Red Hat Enterprise Linux "*" release 6."* ]]; then
                distro_osversion=6
                echo "rhel"
                return 0
            fi
        fi
    fi

    say_verbose "Distribution  OS name  could not be detected: UName = $uname"
    return 1
}

get_distro_os_version() {
    eval $invocation

    if [ -e /etc/os-release ]; then
        . /etc/os-release
        local verid=$VERSION_ID
        if [ -n "$verid" ]; then
            echo "$verid"
            return 0
        fi
    elif [ -e /etc/redhat-release ]; then
        local redhatRelease=$(</etc/redhat-release)
        if [[ $redhatRelease == "CentOS release 6."* || $redhatRelease == "Red Hat Enterprise Linux "*" release 6."* ]]; then
            echo "6"
            return 0
        fi
    fi

    return
}

machine_has() {
    eval $invocation

    hash "$1" >/dev/null 2>&1
    return $?
}

to_lowercase() {
    #eval $invocation

    echo "$1" | tr '[:upper:]' '[:lower:]'
    return 0
}

# args:
# input - $1
remove_trailing_slash() {
    #eval $invocation

    local input="${1:-}"
    echo "${input%/}"
    return 0
}

# args:
# input - $1
remove_beginning_slash() {
    #eval $invocation

    local input="${1:-}"
    echo "${input#/}"
    return 0
}

get_http_header_curl() {
    eval $invocation
    local remote_path="$1"
    remote_path_with_credential="${remote_path}"
    curl_options="-I -sSL --retry 5 --retry-delay 2 --connect-timeout 15 "
    curl $curl_options "$remote_path_with_credential" || return 1
    return 0
}

get_http_header_wget() {
    eval $invocation
    local remote_path="$1"
    remote_path_with_credential="${remote_path}"
    wget_options="-q -S --spider --tries 5 --waitretry 2 --connect-timeout 15 "
    wget $wget_options "$remote_path_with_credential" 2>&1 || return 1
    return 0
}

# args:
# remote_path - $1
# [out_path] - $2 - stdout if not provided
download() {
    eval $invocation

    local remote_path="$1"
    local out_path="${2:-}"

    if [[ "$remote_path" != "http"* ]]; then
        cp "$remote_path" "$out_path"
        return $?
    fi

    local failed=false
    local attempts=0
    while [ $attempts -lt 3 ]; do
        attempts=$((attempts + 1))
        failed=false
        if machine_has "curl"; then
            downloadcurl "$remote_path" "$out_path" || failed=true
        elif machine_has "wget"; then
            downloadwget "$remote_path" "$out_path" || failed=true
        else
            say_err "Missing dependency: neither curl nor wget was found."
            exit 1
        fi

        if [ "$failed" = false ] || [ $attempts -ge 3 ] || { [ ! -z $http_code ] && [ $http_code = "404" ]; }; then
            break
        fi

        say "Download attempt #$attempts has failed: $http_code $download_error_msg"
        say "Attempt #$((attempts + 1)) will start in $((attempts * 10)) seconds."
        sleep $((attempts * 20))
    done

    if [ "$failed" = true ]; then
        say_verbose "Download failed: $remote_path"
        return 1
    fi
    return 0
}

# Updates global variables $http_code and $download_error_msg
downloadcurl() {
    eval $invocation
    unset http_code
    unset download_error_msg
    local remote_path="$1"
    local out_path="${2:-}"

    local curl_options="--retry 20 --retry-delay 2 --connect-timeout 15 -sSL -f --create-dirs "
    local failed=false
    if [ -z "$out_path" ]; then
        curl $curl_options "$remote_path" || failed=true
    else
        curl $curl_options -o "$out_path" "$remote_path" || failed=true
    fi
    if [ "$failed" = true ]; then
        local response=$(get_http_header_curl $remote_path)
        http_code=$(echo "$response" | awk '/^HTTP/{print $2}' | tail -1)
        download_error_msg="Unable to download $remote_path."
        if [[ $http_code != 2* ]]; then
            download_error_msg+=" Returned HTTP status code: $http_code."
        fi
        say_verbose "$download_error_msg"
        return 1
    fi
    return 0
}

# Updates global variables $http_code and $download_error_msg
downloadwget() {
    eval $invocation
    unset http_code
    unset download_error_msg
    local remote_path="$1"
    local out_path="${2:-}"

    local wget_options="--tries 20 --waitretry 2 --connect-timeout 15 "
    local failed=false
    if [ -z "$out_path" ]; then
        wget -q $wget_options -O - "$remote_path" || failed=true
    else
        wget $wget_options -O "$out_path" "$remote_path" || failed=true
    fi
    if [ "$failed" = true ]; then
        local response=$(get_http_header_wget $remote_path)
        http_code=$(echo "$response" | awk '/^  HTTP/{print $2}' | tail -1)
        download_error_msg="Unable to download $remote_path."
        if [[ $http_code != 2* ]]; then
            download_error_msg+=" Returned HTTP status code: $http_code."
        fi
        say_verbose "$download_error_msg"
        return 1
    fi
    return 0
}

get_latest_release_info() {
    eval $invocation

    download "$latesturl"
    return $?

}

# Get Download Url
get_download_url_from_release_info() {
    eval $invocation

    local assets_section=$(cat | awk '/"assets"/,/\],/')

    if [ -z "$assets_section" ]; then
        say_err "Unable to parse the node in "
        return 1
    fi

    local assetslist=$(echo $assets_section | awk -F "[{:}]" ' NF>2 { printf "%s\n",$0}')
    assetslist=${assetslist//[\"\{\} ]/}
    assetslist=${assetslist//,/$'\n'}

    #say_verbose "assets Line: $assetslist"

    local url_info=''
    while read -r line; do
        IFS=:
        while read -r key value; do
            if [[ "$key" == "browser_download_url" && "$value" =~ "$osname-$architecture" ]]; then
                url_info=$value
            fi
        done <<<"$line"
    done <<<"$assetslist"

    if [ -z "$url_info" ]; then
        say_err "Unable to find the download link"
        return 1
    fi

    unset IFS

    echo "$url_info"

    return 0
}

#get download link
get_download_link() {
    eval $invocation

    release_info="$(get_latest_release_info)" || return 1
    say_verbose "get_latest_release_info: release_info=$release_info"

    echo "$release_info" | get_download_url_from_release_info

    return 0
}

# args:
# input_files - stdin
# root_path - $1
# out_path - $2
# override - $3
copy_files_or_dirs_from_list() {
    eval $invocation

    local root_path="$(remove_trailing_slash "$1")"
    local out_path="$(remove_trailing_slash "$2")"
    local override="$3"
    local osname="$(get_current_os_name)"
    local override_switch=$(
        if [ "$override" = false ]; then
            if [ "$osname" = "linux-musl" ]; then
                printf -- "-u"
            else
                printf -- "-n"
            fi
        fi
    )

    cat | uniq | while read -r file_path; do
        local path="$(remove_beginning_slash "${file_path#$root_path}")"
        local target="$out_path/$path"
        if [ "$override" = true ] || ( ! ([ -d "$target" ] || [ -e "$target" ])); then
            mkdir -p "$out_path/$(dirname "$path")"
            if [ -d "$target" ]; then
                rm -rf "$target"
            fi
            cp -R $override_switch "$root_path/$path" "$target"
        fi
    done
}

# args:
# zip_path - $1
# out_path - $2
extract_package() {
    eval $invocation

    local zip_path="$1"
    local out_path="$2"

    local temp_out_path="$(mktemp -d "$temporary_file_template")"

    local failed=false
    unzip "$zip_path" -d "$temp_out_path" >/dev/null || failed=true

    find "$temp_out_path" -type f | sort | copy_files_or_dirs_from_list "$temp_out_path" "$out_path" "$override_files"

    rm -rf "$temp_out_path"
    rm -f "$zip_path" && say_verbose "Temporary zip file $zip_path was removed"

    if [ "$failed" = true ]; then
        say_err "Extraction failed"
        return 1
    fi
    return 0
}

set_permission() {
    eval $invocation

    cat | uniq | while read -r file_path; do
        chmod +x $file_path
    done
}

# args:
# root_path - $1
# appname - $2
set_execution_permission() {

    eval $invocation

    local root_path="$1"
    local filename="$2"

    find "$root_path" -type f -name "$filename" | sort | set_permission

    return 0
}

#Check OS and Architecture
checksystem() {
    eval $invocation

    say "Check System "

    osname="$(get_current_os_name)"
    distro_osname="$(to_lowercase "$(get_distro_os_name)")"
    distro_osversion="$(to_lowercase "$(get_distro_os_version)")"
    architecture="$(to_lowercase "$(get_machine_architecture)")"

    say_verbose "OS      Name: $osname"
    say_verbose "Distro  Name: $distro_osname $distro_osversion"
    say_verbose "Architecture: $architecture"

    if [ ! -z "$osname" ]; then
        case "$osname" in
        freebsd | rhel.6 | linux-musl | linux)
            #echo "$osname"
            return 0
            ;;
        *)
            say_err "'$osname' is not a supported. If you think this is a bug, report it at $issueurl."
            return 1
            ;;

        esac
    fi
    return 0
}

#Check requirements and dependencies
check_reqs() {
    eval $invocation

    say "Check requirements and dependencies"

    local installname=''

    if machine_has "curl"; then
        installname+=''
    else
        installname+=' curl '
        say_verbose "curl Will be installed."
    fi

    if machine_has "unzip"; then
        installname+=''
    else
        installname+=' unzip '
        say_verbose "unzip Will be installed."
    fi

    say "Install dependencies .."

    if [ ! -z "$distro_osname" ]; then
        case "$distro_osname" in
        debian)

            declare -i versionnum=$distro_osversion

            installname+=' libc6 libgcc1 libgssapi-krb5-2 zlib1g libstdc++6 '

            if [ $versionnum -ge 11 ]; then
                installname+=' libssl1.1 libicu67 '
            elif [ $versionnum -ge "10" ]; then
                installname+=' libssl1.1 libicu63'

            elif [ $versionnum -ge "9" ]; then
                installname+=' libssl1.1 libicu57 '

            elif [ $versionnum -ge "8" ]; then
                installname+=' libssl1.0.0 libicu52 '
            else
                say_err "'$distro_osname $distro_osversion' is not a supported. If you think this is a bug, report it at $issueurl."
                return 1
            fi

            say_verbose " install items : $installname"

            apt-get update || return 1
            apt-get -y install $installname || return 1
            apt-get clean || return 1

            return 0
            ;;

        ubuntu)

            installname+=' libc6 libgcc1 libgssapi-krb5-2 zlib1g libstdc++6 '

            declare -i versionnum=${distro_osversion:0:2}

            #"20.04"
            if [ $versionnum -ge 20 ]; then
                installname+=' libssl1.1 libicu66 '

            elif [ $distro_osversion == "18.04" ]; then
                installname+=' libssl1.1 libicu60 '

            elif [ $distro_osversion == "16.04" ]; then
                installname+=' libssl1.0.0 libicu55'

            else
                say_err "'$distro_osname $distro_osversion' is not a supported. If you think this is a bug, report it at $issueurl."
                return 1
            fi

            say_verbose " install items : $installname"

            apt-get update || return 1
            apt-get -y install $installname || return 1
            apt-get clean || return 1

            return 0
            ;;

        alpine)

            local dosver=${distro_osversion%.*}
            declare -i versionnum=${dosver#3.}

            installname+=' bash icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib '

            if [ $versionnum -ge 9 ]; then
                installname+=' libssl1.1 '

            elif [ $versionnum -le 8 ]; then
                installname+=' libssl1.0 '

            else
                say_err "'$distro_osname $distro_osversion' is not a supported. If you think this is a bug, report it at $issueurl."
                return 1
            fi

            say_verbose " install items : $installname"

            apk update || return 1
            apk add --no-cache $installname || return 1

            return 0
            ;;
        centos)

            declare -i versionnum=$distro_osversion

            installname+=' krb5-libs libicu openssl-libs zlib compat-openssl10'

            if [ $versionnum -ge 7 ]; then
                installname+=''
            else
                say_err "'$distro_osname $distro_osversion' is not a supported. If you think this is a bug, report it at $issueurl."
                return 1
            fi

            say_verbose " install items : $installname"

            yum makecache || return 1
            yum -y install $installname || return 1
            yum clean all || return 1

            return 0
            ;;
        fedora | rhel)

            declare -i versionnum=$distro_osversion

            installname+=' krb5-libs libicu openssl-libs zlib compat-openssl10'

            # if [ $(test $distro_osname== "fedora") && $versionnum -ge 27 ]; then
            #     installname+=''
            # elif [ $distro_osname== "rhel" && $versionnum -ge 7 ]; then
            #     installname+=''
            if [ $versionnum -ge 7 ]; then
                installname+=''
            else
                say_err "'$distro_osname $distro_osversion' is not a supported. If you think this is a bug, report it at $issueurl."
                return 1
            fi

            say_verbose " install items : $installname"

            dnf makecache || return 1
            dnf -y install $installname || return 1
            dnf clean all || return 1

            return 0
            ;;
        opensuse-leap | opensuse)

            installname+=' krb5 libicu libopenssl1_0_0 '

            declare -i versionnum=${distro_osversion:0:2}

            if [ $versionnum -ge 15 ]; then
                installname+=''
            else
                say_err "'$distro_osname $distro_osversion' is not a supported. If you think this is a bug, report it at $issueurl."
                return 1
            fi

            say_verbose " install items : $installname"

            zypper refresh || return 1
            zypper install --no-recommends -y $installname || return 1
            zypper clean || return 1

            return 0
            ;;

        *)
            say_err "'$distro_osname' is not a supported. If you think this is a bug, report it at $issueurl."
            return 1
            ;;
        esac

    fi

    return 0
}

#install repo release
install_release() {
    eval $invocation

    local download_failed=false
    local asset_name=''
    local asset_relative_path=''

    zip_path="$(mktemp "$temporary_file_template")"
    say_verbose "Zip path: $zip_path"

    download_link="$(get_download_link)"
    say "Downloading primary link $download_link"

    download "$download_link" "$zip_path" 2>&1 || download_failed=true

    #  if the download fails, download the legacy_download_link
    if [ "$download_failed" = true ]; then
        primary_path_http_code="$http_code"
        primary_path_download_error_msg="$download_error_msg"
        case $primary_path_http_code in
        404)
            say "The resource at $download_link is not available."
            ;;
        *)
            say "$primary_path_download_error_msg"
            ;;
        esac
        rm -f "$zip_path" 2>&1 && say_verbose "Temporary zip file $zip_path was removed"

    fi

    say "Extracting zip from $download_link"

    extract_package "$zip_path" "$install_root" || return 1

    set_execution_permission "$install_root" "$appname"

    say "Installation finished successfully."

    return 0
}

runall() {
    checksystem
    check_reqs
    install_release

}

args=("$@")
script_name=$(basename "$0")

while [ $# -ne 0 ]; do
    name="$1"
    case "$name" in
    --verbose | -[Vv]erbose)
        verbose=true
        non_dynamic_parameters+=" $name"
        ;;

    -? | --? | -h | --help | -[Hh]elp)
        script_name="$(basename "$0")"
        echo "Tools Installer"
        echo "Usage: $script_name [verbose] "
        echo "       $script_name -h|-?|--help"
        echo ""
        echo "Options:"
        echo "  --verbose,-Verbose                 Display diagnostics information."
        echo "  -?,--?,-h,--help,-Help             Shows this help message"
        echo ""
        exit 0
        ;;
    *)
        say_err "Unknown argument \`$name\`"
        exit 1
        ;;
    esac

    shift
done

runall
