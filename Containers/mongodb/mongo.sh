            if [ $(az network dns record-set list -g dnsmanagement -z  ${{ secrets.ENV_DOMAIN }}  --query "[?name=='$arecord']|  length(@)") = 0 ]; then
              echo "create resource" $arecord
              az network dns record-set a add-record -g dnsmanagement -z ${{ secrets.ENV_DOMAIN }} -n $arecord -a $EXTERNAL_IP
            else
              echo "update resource" $arecord
              oldIP=$(az network dns record-set list -g dnsmanagement -z myrcan.com  --query "[?name=='$arecord'] | [].aRecords[].ipv4Address | [0]")
              
              
                modified="${oldIP:1:-1}"
              
              echo ${$oldIP::-1} && echo "$EXTERNAL_IP"
              if [ "$oldIP" = "$EXTERNAL_IP" ]; then
                echo "Different IP del create new"
                az network dns record-set a add-record -g dnsmanagement -z ${{ secrets.ENV_DOMAIN }} -n $arecord --ipv4-address $EXTERNAL_IP
                az network dns record-set a remove-record -g dnsmanagement -z ${{ secrets.ENV_DOMAIN }} -n $arecord --ipv4-address $oldIP
              else
                echo "Same IP no update"
              fi
            fi
